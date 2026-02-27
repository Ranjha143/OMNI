using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omni_Courier_Service.Watchers;
using Oracle.ManagedDataAccess.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Omni_Courier_Service.Services
{
    internal class C3X
    {
        public static async void AssignCourier(string order_id, ILogger<CourierAssignment> _logger, IMongoDatabase mongoDB)
        {
            try
            {
                var OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");
                var countryCollection = mongoDB.GetCollection<BsonDocument>("countries");

               

                var filterQuery = @$"{{order_id:{order_id}}}";
                var orderResult = await OrderCollection.Find(filterQuery).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var orderInfo = JsonConvert.DeserializeObject<List<OrderInfo>>(JsonConvert.SerializeObject(orderObj))?.FirstOrDefault();

                var OrderCourier = mongoDB.GetCollection<BsonDocument>("courier");
                var courierInfo = await OrderCourier.Find("{courier_name:'C3X'}").FirstOrDefaultAsync();

                var storeSidQuery = $@"select store_no , store_name,ADDRESS1,ADDRESS2, ADDRESS3,ADDRESS4,ADDRESS5, PHONE1, UDF1_STRING from rps.store where sid = {orderInfo.assigned_store_sid} ";
                List<JObject> storeSIdObj = ADO.ReadAsync<JObject>(storeSidQuery);
               
                var SendersAddress1 = storeSIdObj[0]["ADDRESS1"]?.ToString();
                var SendersAddress2 = storeSIdObj[0]["ADDRESS2"]?.ToString();
                var SendersCity = storeSIdObj[0]["ADDRESS4"]?.ToString();
                var SendersCountry = storeSIdObj[0]["ADDRESS5"]?.ToString();
                var SendersPhone = storeSIdObj[0]["PHONE1"]?.ToString();
                var senderEmail = storeSIdObj[0]["UDF1_STRING"]?.ToString();

                var pload = new
                {
                    AirwayBillData = new
                    {
                        CODAmount = orderInfo.TotalOutstandingSet.ShopMoney.Amount,
                        // CODAmount = orderInfo.PaymentGatewayNames.FirstOrDefault() == "Cash on Delivery (COD)" ? orderInfo.TotalPriceSet.ShopMoney.Amount : 0,
                        CODCurrency = orderInfo.TotalPriceSet.ShopMoney.CurrencyCode,
                        Destination = "DXB",
                        DutyConsigneePay = "0",
                        GoodsDescription = string.Join(",", orderInfo.LineItemList.Select(s => s.Sku).ToList()),
                        NumberofPeices = 1,
                        Origin = "DXB",
                        ProductType = "XPS",
                        ReceiversAddress1 = orderInfo.ShippingAddress.Address1,
                        ReceiversAddress2 = orderInfo.ShippingAddress.Address2 == null ? orderInfo.ShippingAddress.Address1 : orderInfo.ShippingAddress.Address2,
                        ReceiversCity = orderInfo.ShippingAddress.City,
                        ReceiversSubCity = "",
                        ReceiversCountry = "AE",
                        ReceiversCompany = orderInfo.ShippingAddress.Name,
                        ReceiversContactPerson = orderInfo.ShippingAddress.Name,
                        ReceiversEmail = orderInfo.Customer.Email,
                        ReceiversGeoLocation = $"{orderInfo.ShippingAddress.Latitude},{orderInfo.ShippingAddress.Longitude}",
                        ReceiversMobile = orderInfo.ShippingAddress.Phone,
                        ReceiversPhone = orderInfo.ShippingAddress.Phone,
                        ReceiversPinCode = "", 
                        SendersAddress1 = SendersAddress1,
                        SendersAddress2 = SendersAddress2 == null ? SendersAddress1 : SendersAddress2,
                        SendersCity = SendersCity,
                        SendersCountry = SendersCountry,
                        SendersCompany = "LOGO | OPIA",
                        SendersContactPerson = " ",
                        SendersEmail = senderEmail,
                        SendersMobile = SendersPhone,
                        SendersPhone = SendersPhone,
                        SendersPinCode = "000000",
                        ServiceType = orderInfo.TotalOutstandingSet.ShopMoney.Amount > 0 ? "CAD":"NOR" ,
                        //ServiceType = orderInfo.PaymentGatewayNames.FirstOrDefault() == "Cash on Delivery (COD)" ? "CAD" : "NOR",
                        ShipmentDimension = "1X1X1",
                        ShipmentInvoiceCurrency = orderInfo.TotalPriceSet.ShopMoney.CurrencyCode,
                        ShipmentInvoiceValue = orderInfo.TotalPriceSet.ShopMoney.Amount,
                        ShipperReference = orderInfo.Name,
                        ShipperVatAccount = "",
                        SpecialInstruction = "",
                        Weight = 0.5
                    },
                    UserName = courierInfo["user_name"], //"SIRIUS",
                    Password = courierInfo["password"], //"SIRIUS@293",
                    AccountNo = courierInfo["account_no"], //"10293",
                    Country =  courierInfo["Country"], //"AE"
                };

                var payload = JsonConvert.SerializeObject(pload);

                var cnResult = await RestApiClient.SendAsync("https://portal.c3xpress.com/C3XService.svc", "/CreateAirwayBill", RestSharp.Method.Post, null, null, JsonConvert.SerializeObject(pload), false);

                if (cnResult.IsSuccessful)
                {
                    var CNdata = JsonConvert.DeserializeObject<CourierResponse>(cnResult.Content ?? "{}");

                    if (CNdata?.Description == "Success")
                    {
                        var update = Builders<BsonDocument>.Update.Set("accepted_by_store", "accepted").Set("courier.courier_name", "C3X").Set("courier.cn_number", CNdata.AirwayBillNumber).Set("is_courier_assigned",true);
                        var result = await OrderCollection.UpdateOneAsync(filterQuery, update);

                        var assignEvent = new EventNode
                        {
                            Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                            Message = $"Order {orderInfo.Name} with Id {orderInfo.OrderId} is assigned Courier No by C3X",
                            CreatedAt = DateTime.UtcNow,
                            __typename = "OperationalEvent",
                            Action = "Courier Assigned",
                            SubjectId = "gid://shopify/Order/" + orderInfo.OrderId,
                            SubjectType = "ORDER"
                        };

                        var assignEventDoc = assignEvent.ToBsonDocument();
                        var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                        _ = await OrderCollection.UpdateOneAsync(filterQuery, shopifyOrderUpdate);

                    }
                    else
                    {
                        _logger.LogError("Error gettoing CN from C3X. Response from Courier: {response} ", cnResult.Content);
                    }
                }
                else
                {
                    _logger.LogError("Error getting CN from C3X. Response from Courier: {response} ", cnResult.Content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing C3X");
                //  throw;
            }
        }

    }
}




