using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;
using Quartz;
using ShopifySharp;
using ShopifySharp.Filters;
using System.ComponentModel;

namespace Shopify
{
    public class Order_via_API : IJob
    {

        readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public Order_via_API()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;

        }

        public async Task Execute(IJobExecutionContext context)
        {
            var workerTask = Task.Factory.StartNew(() => LoadConfigurations().Wait());
            Task.WaitAll(workerTask);

            if (!GlobalVariables.ShopifyOrderWorker && GlobalVariables.OrderServiceIsEnabled)
            {
                GlobalVariables.ShopifyOrderWorker = true;
                await Task.Delay(0);
                threadWorker.RunWorkerAsync();
            }
        }

        private async Task<bool> LoadConfigurations()
        {


            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");


            var serviceFilter = $@"{{""service"":""Order""}}";
            var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
            if (serviceResult.Count > 0)
            {
                var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault() ?? new();
                GlobalVariables.OrderServiceIsEnabled = ServiceInfo.Enabled;
                GlobalVariables.OrderServiceInterval = ServiceInfo.Interval;
            }

            return true;

        }

        private void ThreadWorker_DoWork(object? sender, DoWorkEventArgs? e)
        {
            var workerTask = Task.Factory.StartNew(() => ProcessOrders().Wait());
            workerTask.Wait();
        }

        private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs? e)
        {

        }
        private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs? e)
        {
            GlobalVariables.ShopifyOrderWorker = false;
        }

        private async Task<bool> ProcessOrders()
        {

            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                var shopifyConfig = GlobalVariables.ShopifyConfig;

                // ====================  sale order  new and fulfilled ==========================
                var OrderCollection = mongoDB.GetCollection<BsonDocument>("orders");
                var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("orders_log");
                var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

                var ShopifyOrderWorkerTask = Task.Factory.StartNew(() => GetShopifyOrder("Orders", OrderCollection, OrderLogCollection).Wait());
                ShopifyOrderWorkerTask.Wait();

                //ShopifyOrderWorkerTask = Task.Factory.StartNew(() => GetShopifyOrder("Invoices", OrderCollection, OrderLogCollection).Wait());
                //ShopifyOrderWorkerTask.Wait();
                //// ====================  refunded order ==========================
                //if (shopifyConfig.SaleRefundDirection == "pull")
                //{
                //    var workerTask = Task.Factory.StartNew(() => GetShopifyOrder("REFUNDED", OrderCollection, OrderLogCollection).Wait());
                //    workerTask.Wait();
                //}
                //// ====================  cancelled order ==========================
                //if (shopifyConfig.SaleCancellationDirection == "pull")
                //{
                //    var workerTask = Task.Factory.StartNew(() => GetShopifyOrder("Cancelled", OrderCollection, OrderLogCollection).Wait());
                //    workerTask.Wait();
                //}

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private async Task<bool> GetShopifyOrder(string service, IMongoCollection<BsonDocument> OrderCollection, IMongoCollection<BsonDocument> OrderLogCollection)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");
            var CityCollection = mongoDB.GetCollection<BsonDocument>("cities");

            if (service.ToUpper() == "ORDERS")
            {
                OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");
            }
            else if (service.ToUpper() == "INVOICES")
            {
                OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_invoices");
                OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_invoices_log");
            }
            else if (service.ToUpper() == "REFUNDED")
            {
                OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_refund_orders");
                OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_refund_orders_log");
            }
            else if (service.ToUpper() == "CANCELLED")
            {
                OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders");
                OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders_log");
            }

            try
            {

                var shopifyConfig = GlobalVariables.ShopifyConfig;
                if (shopifyConfig != null)
                {

                    var filter = @$"{{type:'{service}'}}";
                    var logResult = await OrderLogCollection.Find(filter).ToListAsync();
                    var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();

                    long? lastOrderId = null;
                    DateTimeOffset LastUpdated = DateTimeOffset.UtcNow;
                    var startTime = DateTime.Now;
                    if (log == null)
                    {
                        BsonDocument document = new BsonDocument();
                        document["type"] = service;
                        document["created_at"] = DateTime.Now;
                        document["last_updated_at"] = DateTime.Now;
                        document["total_orders"] = 0;
                        document["processed"] = 0;
                        document["last_order_id_fetched"] = 0;
                        document["failed"] = 0;
                        await OrderLogCollection.InsertOneAsync(document);
                        lastOrderId = 0;
                        LastUpdated = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        _ = DateTimeOffset.TryParse(log["last_updated_at"]?.ToString(), out DateTimeOffset LastUpdatedAt);

                        if (LastUpdatedAt != DateTimeOffset.MinValue)
                        {
                            LastUpdated = LastUpdatedAt;
                        }
                    }

                    var orderService = new OrderService($"{GlobalVariables.ShopifyConfig.StoreIdentifier}.myshopify.com", GlobalVariables.ShopifyConfig.ApiAccessToken);
                    var transactionService = new TransactionService($"{GlobalVariables.ShopifyConfig.StoreIdentifier}.myshopify.com", GlobalVariables.ShopifyConfig.ApiAccessToken);

                    List<Order> allOrders = new List<Order>();
                    do
                    {
                        List<Order> fulfilled = new List<Order>();
                        List<Order> newOrders = new List<Order>();

                        if (GlobalVariables.ShopifyConfig.SaleOrderFetchState?.ToLower() == "new")
                        {
                            var orders = (await orderService.ListAsync(new OrderListFilter
                            {
                                Status = "any",
                                Limit = 200,
                                SinceId = lastOrderId,
                                CreatedAtMin = LastUpdated.AddMinutes(-5)
                            })).Items.ToList();

                            if (orders.Any())
                            {
                                lastOrderId = orders.Last().Id;
                            }
                            else
                            {
                                lastOrderId = null;
                            }

                            newOrders = orders.Where(order => order.FulfillmentStatus == null).ToList();
                            allOrders.AddRange(fulfilled);

                        }
                        else if (GlobalVariables.ShopifyConfig.SaleOrderFetchState?.ToLower() == "fulfilled")
                        {
                            var orders = (await orderService.ListAsync(new OrderListFilter
                            {
                                Status = "any",
                                Limit = 200,
                                SinceId = lastOrderId,
                                UpdatedAtMin = LastUpdated.AddMinutes(-1)
                            })).Items.ToList();

                            if (orders.Any())
                            {
                                lastOrderId = orders.Last().Id;
                            }
                            else
                            {
                                lastOrderId = null;
                            }

                            fulfilled = orders.Where(order => order.FulfillmentStatus != null && order.FulfillmentStatus.Equals("fulfilled")).ToList();
                            allOrders.AddRange(fulfilled);
                        }


                    } while (lastOrderId.HasValue);

                    var checkDate = DateTimeOffset.Now.AddDays(-30);
                    int shopifyOrderCount = 200;
                    shopifyOrderCount = await orderService.CountAsync(new OrderCountFilter { UpdatedAtMin = checkDate });

                    var CheckOrdersList = (await orderService.ListAsync(new OrderListFilter
                    {
                        Status = "any",
                        UpdatedAtMin = checkDate
                    })).Items.ToList();
                    var shopifyCheckIds = CheckOrdersList.Where(s => s.FulfillmentStatus != null && s.FulfillmentStatus == "fulfilled").Select(s => s.Id.ToString()).ToList();

       
                    var projection = Builders<BsonDocument>.Projection.Include("id").Exclude("_id");
                    var filterX = $"{{updated_at:{{$gt:'{checkDate.ToString("yyyy-MM-dd")}'}}}}";
                    var orderIdsResult = await OrderCollection.Find(filterX).Project(projection).ToListAsync();
                    var orderIdsObject = orderIdsResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var orderIdJObject = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(orderIdsObject)).ToList();
                    var orderIds = orderIdJObject.Select(s => s["id"].ToString()).ToList();
                    List<string> missingIds = shopifyCheckIds.Except(orderIds).ToList();

                    foreach (var id in missingIds)
                    {
                        var order = await orderService.GetAsync(long.Parse(id));
                        var name = order.Name;
                        var filter2 = "{name:'" + name + "'}";
                        var projection2 = Builders<BsonDocument>.Projection.Include("name").Exclude("_id");
                        var orderResult = await OrderCollection.Find(filter2).Project(projection2).ToListAsync();
                        if (orderResult.Count == 0 && order.FulfillmentStatus != null && order.FulfillmentStatus == "fulfilled")
                        {
                            allOrders.Add(order);
                        }
                    }

                    // ===========================================>> Check for Missing Orders

                    allOrders = allOrders.Distinct().ToList();
                    foreach (var order in allOrders.Distinct().ToList())
                    {
                        try
                        {
                            var orderFound = (await OrderCollection.FindAsync("{id:" + order.Id + "}")).ToList();

                            if (!orderFound.Any() && order.FulfillmentStatus != null && order.FulfillmentStatus.Equals("fulfilled"))
                            {
                                var transactions = await transactionService.ListAsync(order.Id ?? 0);
                                if (transactions.Any())
                                    order.Transactions = transactions;

                                var doocument = BsonDocument.Parse(JsonConvert.SerializeObject(order));
                                doocument["CreatedAt"] = new BsonDateTime(order.CreatedAt.Value.UtcDateTime);
                                doocument["Synced"] = new BsonBoolean(false);
                                doocument["RetailProSid"] = new BsonInt64(-1);
                                doocument["invoiced"] = new BsonBoolean(false);
                                doocument["refunded"] = new BsonBoolean(false);


                                var destinationAddress = order.ShippingAddress.Address1 + order.ShippingAddress.Address2;
                                doocument.Add(new BsonElement("courier", new BsonDocument { { "courier_name", "" }, { "cn_number", "" }, { "destination_city", order.ShippingAddress.City }, { "destination_address", destinationAddress } }));
                                doocument.Add(new BsonElement("courier_error", new BsonDocument { { "courier_error", false }, { "short_inventory", false }, { "error_message", "" } }));
                                doocument.Add(new BsonElement("order_error_state", false));

                                doocument.Add(new BsonElement("order_source", "Shopify"));
                                //doocument.Add(new BsonElement("printed", 0));
                                doocument.Add(new BsonElement("fulfullment_sent", false));
                                doocument.Add(new BsonElement("invoice", new BsonDocument { { "created", 0 }, { "invoice_Id", 0 }, { "invoice_synced", 0 }, { "invoice_synced_dateTime", DateTime.Now }, { "synced_invoice_sid", 0 } }));


                                doocument.Add(new BsonElement("courier_retry", DateTime.Now.AddMinutes(5)));
                                doocument.Add(new BsonElement("is_courier_assigned", false));

                                var cityProjection = Builders<BsonDocument>.Projection.Include("name").Exclude("_id");
                                var cityResult = await CityCollection.Find($@"{{country_name:{{$in:[""Pakistan""]}}}}").Project(cityProjection).ToListAsync();
                                var cityResultObject = cityResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                                var CityList = JsonConvert.DeserializeObject<List<CityModel>>(JsonConvert.SerializeObject(cityResultObject))?.Select(s => s.CityName).ToList() ?? [];

                                var possibleCityList = CityList.Where(c => Soundex.For(c) == Soundex.For(order.ShippingAddress.City)).ToList();
                                var possibleDestinationCity = possibleCityList.FirstOrDefault(c => LevenshteinDistance.CalculateSimilarity(c, order.ShippingAddress.City) > 80);

                                if (string.IsNullOrEmpty(possibleDestinationCity))
                                {
                                    doocument.Add(new BsonElement("order_tracking_status", new BsonDocument { { "status_code", 3030 }, { "status", "INVALID CITY" }, { "status_updated_at", DateTime.Now } }));
                                    doocument.Add(new BsonElement("order_tracking_history", new BsonArray { new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } }, new BsonDocument { { "status_code", 3030 }, { "status", "INVALID CITY" }, { "order_error_state", true }, { "status_updated_at", DateTime.Now } } }));
                                }
                                else
                                {
                                    if (GlobalVariables.CourierServiceisAvailable)
                                    {
                                        doocument.Add(new BsonElement("order_tracking_status", new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } }));
                                        doocument.Add(new BsonElement("order_tracking_history", new BsonArray { new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } } }));
                                    }
                                    else
                                    {
                                        doocument.Add(new BsonElement("order_tracking_status", new BsonDocument { { "status_code", 2000 }, { "status", "APPROVED" }, { "status_updated_at", DateTime.Now } }));
                                        doocument.Add(new BsonElement("order_tracking_history", new BsonArray { new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } }, new BsonDocument { { "status_code", 2000 }, { "status", "APPROVED" }, { "status_updated_at", DateTime.Now } } }));
                                    }
                                }




                                doocument["posting_retry"] = new BsonInt32(0);
                                doocument["has_error"] = new BsonBoolean(false);
                                await OrderCollection.InsertOneAsync(doocument);


                                TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                                DateTime swissLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, swissTimeZone);
                                var exceptionUpdate = Builders<BsonDocument>.Update
                                            .Inc("total_orders", 1)
                                            .Inc("processed", 1)
                                            .Set("last_updated_at", swissLocalTime.ToString());
                                await OrderLogCollection.UpdateOneAsync(filter, exceptionUpdate);

                                BsonDocument document = new BsonDocument();
                                document["type"] = "Shopify Order Pull";
                                document["created_at"] = DateTime.Now;
                                document["order_no"] = order.Name;
                                await OrderLogCollection.InsertOneAsync(document);

                            }
                        }
                        catch (Exception ex)
                        {
                           
                            BsonDocument document = new BsonDocument();
                            document["service"] = "Fetch Shopify Orders";
                            document["created_at"] = DateTime.Now;
                            document["order_number"] = order.Name;
                            document["exception_message"] = ex.Message;
                            document["exception_source"] = ex.Source;
                            document["exception_stack_trace"] = ex.StackTrace;
                            document["seen"] = false;
                            await exceptionCollection.InsertOneAsync(document);

                            TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                            DateTime swissLocalTime = TimeZoneInfo.ConvertTime(DateTime.Now, swissTimeZone);
                            var exceptionUpdate = Builders<BsonDocument>.Update
                                        .Inc("total_orders", 1)
                                        .Inc("failed", 1)
                                        .Set("last_updated_at", swissLocalTime.ToString());
                            await OrderLogCollection.UpdateOneAsync(filter, exceptionUpdate);

                        }

                    }

                    TimeZoneInfo swissTimeZone2 = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    DateTime swissLocalTime2 = TimeZoneInfo.ConvertTime(DateTime.Now, swissTimeZone2);


                    if (allOrders.Any())
                    {
                        lastOrderId = allOrders.OrderBy(o => o.Id).LastOrDefault()?.Id ?? 0;
                        var update = Builders<BsonDocument>.Update
                        .Set("last_order_id_fetched", lastOrderId)
                        .Set("last_updated_at", swissLocalTime2.ToString());
                        await OrderLogCollection.UpdateOneAsync(filter, update);
                    }
                    else
                    {
                        var update = Builders<BsonDocument>.Update
                       .Set("last_updated_at", swissLocalTime2.ToString());
                        await OrderLogCollection.UpdateOneAsync(filter, update);
                    }

                    var elapsedTime = Math.Round((DateTime.Now - startTime).TotalMinutes, 2);
                    var updateElapsedtime = Builders<BsonDocument>.Update.Set("total_time_elapsed", elapsedTime);
                    await OrderLogCollection.UpdateOneAsync(filter, updateElapsedtime);


                }
            }
            catch (Exception ex)
            {

                BsonDocument document = new BsonDocument();
                document["service"] = service;
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }
            return true;
        }



    }
}
