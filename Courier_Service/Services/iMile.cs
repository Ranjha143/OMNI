using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omni_Courier_Service.Watchers;
using RestSharp;
using System.Runtime.InteropServices.JavaScript;


namespace Omni_Courier_Service.Services
{
    public class iMile
    {

        public static async void AssignCourier(string order_id, ILogger<CourierAssignment> _logger, IMongoDatabase _mongoDB)
        {
            try
            {
                string mongoConnectionString = ScopeVariables.MongoConnectionString;
                MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(ScopeVariables.MongoDatabase);

                var OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");

                var countryCollection = mongoDB.GetCollection<BsonDocument>("countries");

                var OrderCourier = mongoDB.GetCollection<BsonDocument>("courier");

                //var filterQuery = "{posted:false, has_error:false, is_courier_assigned:false, accepted_by_store:'accepted'}";
                var filterQuery = @$"{{order_id:{order_id}}}";
                var orderResult = await OrderCollection.Find(filterQuery).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var Order = JsonConvert.DeserializeObject<List<OrderInfo>>(JsonConvert.SerializeObject(orderObj))?.FirstOrDefault();

                var courierInfo = await OrderCourier.Find("{courier_name:'iMile'}").FirstOrDefaultAsync();
                var courierBaseUrl = courierInfo["courier_url"].ToString();


                //TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                string formattedTimeZoneOffset = $"{(offset.Hours >= 0 ? "+" : "-")}{offset.Hours:D2}";

                long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var tokenPayload = new
                {
                    customerId = courierInfo["customerId"].ToString(),
                    sign = courierInfo["sign"].ToString(), //"MIICdQIBADANBgkqhkiG9w0BAQEFAASCAl8wggJbAgEAAoGBAIuTQjg8rVUldwxB",
                    signMethod = "SimpleKey",
                    format = "json",
                    version = "1.0.0",
                    timestamp = unixTimestamp,
                    timeZone = formattedTimeZoneOffset,
                    param = new
                    {
                        grantType = "clientCredential"
                    }
                };


                var response = await RestApiClient.SendAsync(
                    courierInfo["courier_url"].ToString() ?? "",
                    $"/auth/accessToken/grant",
                    Method.Post,
                    body: tokenPayload,
                    headers: new Dictionary<string, string>
                        {
                            { "User-Agent", "WOMS/1.0" }
                        }
                );
                var tokenObject = JsonConvert.DeserializeObject<JObject>(response.Content);

                if (tokenObject["message"].ToString() == "success")
                {



                    var authToken = tokenObject["data"]["accessToken"].ToString();

                    var skuInfo = new List<Object>();

                    foreach (var item in Order.LineItemList)
                    {
                        skuInfo.Add(new
                        {
                            skuNo = item.Sku,
                            skuName = item.Name,
                            skuDesc = item.Title,
                            skuQty = item.Quantity,
                            skuDeclaredValue = item.DiscountedUnitPriceAfterAllDiscountsSet.ShopMoney.Amount,
                            skuWeight = "0.5",
                            skuVolume = "",
                            skuHsCode = "",
                            skuUrl = ""
                        });
                    }


                    var payload = new
                    {
                        customerId = courierInfo["customerId"].ToString(),
                        sign = courierInfo["sign"].ToString(),
                        accessToken = authToken,
                        signMethod = "SimpleKey",
                        format = "json",
                        version = "1.0.0",
                        timestamp = unixTimestamp,
                        timeZone = formattedTimeZoneOffset,
                        param = new
                        {
                            orderNo = Order?.Name,
                            orderType = "100",
                            originalWaybillNo = "",
                            senderInfo = new
                            {
                                addressType = "Seller",
                                contactCompany = "LOGO",
                                contacts = "",
                                phone = Order.SenderInfo.SendersPhone,
                                backupPhone = "",
                                country = "UAE",
                                province = "",
                                city = Order.SenderInfo.SendersCity,
                                area = "",
                                address = (Order.SenderInfo.SendersAddress1 + " " + Order.SenderInfo.SendersAddress2).Trim(),
                                longitude = "",
                                latitude = "",
                                email = Order.SenderInfo.SenderEmail,
                                suburb = "",
                                zipCode = "",
                                street = "",
                                externalNo = "",
                                internalNo = ""
                            },
                            consigneeInfo = new
                            {
                                addressType = "customer",
                                contactCompany = "",
                                contacts = Order.ShippingAddress.Name,
                                phone = Order.ShippingAddress.Phone,
                                backupPhone = Order.BillingAddress.Phone,
                                email = Order.Email,
                                country = Order.ShippingAddress.Country,
                                province = "",
                                city = Order.ShippingAddress.City,
                                area = "",
                                address = (Order.ShippingAddress.Address1 + " " + Order.ShippingAddress.Address2).Trim(),
                                longitude = "",
                                latitude = "",
                                suburb = "",
                                zipCode = "",
                                street = "",
                                externalNo = "",
                                internalNo = "",
                                idNo = "",
                                idCardFrontImg = "",
                                idCardBackImg = ""
                            },
                            packageInfo = new
                            {
                                goodsType = "Normal",
                                paymentMethod = Order.TotalOutstandingSet.ShopMoney.Amount > 0 ? "COD" : "PPD",
                                // paymentMethod = Order.PaymentGatewayNames.FirstOrDefault() == "Cash on Delivery (COD)" ? "COD" : "PPD",
                                collectingMoney = Order.TotalOutstandingSet.ShopMoney.Amount,
                                clientDeclaredValue = Order.TotalPriceSet.ShopMoney.Amount,
                                clientDeclaredCurrency = "Local",
                                productValue = Order.TotalPriceSet.ShopMoney.Amount,
                                productValueCurrency = "Local",
                                length = "",
                                width = "",
                                high = "",
                                totalVolume = Order.TotalPriceSet.ShopMoney.Amount,
                                grossWeight = "1.5"
                            },
                            skuInfos = skuInfo
                        }
                    };


                    var cnResponse = await RestApiClient.SendAsync(
                       courierInfo["courier_url"].ToString() ?? "",
                       $"/client/order/v2/createOrder",
                       Method.Post,
                       body: payload,
                       headers: new Dictionary<string, string>
                           {
                            { "User-Agent", "WOMS/1.0" }
                           }
                   );


                    _logger.LogError($"Courier CN Response {cnResponse.Content}");

                    var expressNoObject = JsonConvert.DeserializeObject<JObject>(cnResponse.Content);

                    if (tokenObject["message"].ToString() == "success")
                    {
                        var expressNo = expressNoObject["data"]["expressNo"].ToString();

                        var orderUpdate = Builders<BsonDocument>.Update
                            .Set("courier.courier_name", courierInfo["courier_name"].ToString())
                            .Set("courier.cn_number", expressNo)
                            .Set("is_courier_assigned", true)
                            .Set("courier.updated_at", DateTimeOffset.UtcNow);

                        _ = await OrderCollection.UpdateOneAsync(filterQuery, orderUpdate);


                        var assignEvent = new EventNode
                        {
                            Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                            Message = $"Order {Order.Name} with Id {Order.OrderId} is assigned Courier No by iMile",
                            CreatedAt = DateTime.UtcNow,
                            __typename = "OperationalEvent",
                            Action = "Courier Assigned",
                            SubjectId = "gid://shopify/Order/" + Order.OrderId,
                            SubjectType = "ORDER"
                        };

                        var assignEventDoc = assignEvent.ToBsonDocument();
                        var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                        _ = await OrderCollection.UpdateOneAsync(filterQuery, shopifyOrderUpdate);

                    }
                    else
                    {
                        _logger.LogError($"Error making iMile Create Order Api Call{response.Content}");
                    }
                }
                else
                {
                    _logger.LogError($"Error making iMile Token Api Call{response.Content}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,  $"Error executing iMile");
               //  throw;
            }
        }
    }
}
