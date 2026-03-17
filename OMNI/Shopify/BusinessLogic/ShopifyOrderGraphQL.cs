using GraphQL;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMNI.Shopify.BusinessLogic;
using OMNI_Dashboard.ApiControllers;
using PluginManager;
using Quartz;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;

namespace Shopify
{
    [DisallowConcurrentExecution]
    internal class ShopifyOrderGraphQl : IJob
    {
        private readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public ShopifyOrderGraphQl()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;
        }

        public async Task Execute(IJobExecutionContext context)
        {


            if (!GlobalVariables.ShopifyOrderWorker)  //  && GlobalVariables.OrderServiceIsEnabled
            {
                //var workerTask = Task.Factory.StartNew(() => LoadConfigurations().Wait());
                //Task.WaitAll(workerTask);
                GlobalVariables.ShopifyOrderWorker = true;
                await Task.Delay(0);
                threadWorker.RunWorkerAsync();
            }
        }

        //private async Task<bool> LoadConfigurations()
        //{
        //    MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
        //    var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
        //    IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");
        //    var serviceFilter = $@"{{""service"":""Order""}}";
        //    var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
        //    if (serviceResult.Count > 0)
        //    {
        //        var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
        //        var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault() ?? new();
        //        GlobalVariables.OrderServiceIsEnabled = ServiceInfo.Enabled;
        //        GlobalVariables.OrderServiceInterval = ServiceInfo.Interval;
        //    }

        //    return true;
        //}

        private void ThreadWorker_DoWork(object? sender, DoWorkEventArgs? e)
        {


#if DEBUG

            var workerTask = Task.Factory.StartNew(() => ProcessOrders().Wait());
            workerTask.Wait();

            var eventWorkerTask = Task.Factory.StartNew(() => GetOrderEvents().Wait());
            eventWorkerTask.Wait();

#endif


#if !DEBUG

            var workerTask = Task.Factory.StartNew(() => ProcessOrders().Wait());
            workerTask.Wait();

            var eventWorkerTask = Task.Factory.StartNew(() => GetOrderEvents().Wait());
            eventWorkerTask.Wait();

#endif



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

                var ShopifyOrderWorkerTask = Task.Factory.StartNew(() => GetShopifyOrder("Orders", OrderCollection, OrderLogCollection).Wait());
                ShopifyOrderWorkerTask.Wait();

                var ShopifyINVOICESTask = Task.Factory.StartNew(() => GetShopifyOrder("INVOICES", OrderCollection, OrderLogCollection).Wait());
                ShopifyINVOICESTask.Wait();

                // ====================  refunded order ==========================
                if (shopifyConfig.SaleRefundDirection == "pull")
                {
                    var workerTask = Task.Factory.StartNew(() => GetShopifyOrder("REFUNDED", OrderCollection, OrderLogCollection).Wait());
                    workerTask.Wait();
                    var RETURNEDTask = Task.Factory.StartNew(() => GetShopifyOrder("RETURNED", OrderCollection, OrderLogCollection).Wait());
                    RETURNEDTask.Wait();
                    var partially_refunded_Task = Task.Factory.StartNew(() => GetShopifyOrder("Partially_Refunded", OrderCollection, OrderLogCollection).Wait());
                    partially_refunded_Task.Wait();
                }

                // ====================  cancelled order ==========================
                if (shopifyConfig.SaleCancellationDirection == "pull")
                {
                    var workerTask = Task.Factory.StartNew(() => GetShopifyOrder("Cancelled", OrderCollection, OrderLogCollection).Wait());
                    workerTask.Wait();
                }

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
            var shopifyOrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");

            if (service.ToUpper() == "ORDERS" || service.ToUpper() == "INVOICES")
            {
                OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");
            }
            //else if (service.ToUpper() == "INVOICES")
            //{
            //    OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_invoices");
            //    OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_invoices_log");
            //}partially_refunded
            else if (service.ToUpper() == "REFUNDED" || service.ToUpper() == "RETURNED" || service.ToUpper() == "PARTIALLY_REFUNDED")
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
                    DateTimeOffset LastUpdated = DateTimeOffset.Now;
                    var startTime = DateTime.Now;
                    if (log == null)
                    {
                        BsonDocument document = new()
                        {
                            ["type"] = service,
                            ["created_at"] = DateTime.Now,
                            ["last_updated_at"] = DateTime.Now,
                            ["total_orders"] = 0,
                            ["processed"] = 0,
                            ["failed"] = 0
                        };
                        await OrderLogCollection.InsertOneAsync(document);
                        LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        _ = DateTimeOffset.TryParse(log["last_updated_at"]?.ToString(), out DateTimeOffset LastUpdatedAt);

                        if (LastUpdatedAt != DateTimeOffset.MinValue)
                        {
                            LastUpdated = LastUpdatedAt;
                        }
                    }

                    int orderCount = 10;
                    bool hasNextRecord = true;
                    string endCursor = string.Empty;
                    List<OrderNode> orderNodes = [];

                    TimeZoneInfo muscatZone = TimeZoneInfo.FindSystemTimeZoneById(GlobalVariables.ShopifyConfig.Timezone);
                    DateTime storeTime = TimeZoneInfo.ConvertTimeFromUtc(LastUpdated.DateTime, muscatZone);
                    var createdAt = storeTime.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ss");
                    
                    while (hasNextRecord)
                    {// financial_status:'paid' AND
                        var QueryParam = "";



                        
                       if (service.ToUpper() == "ORDERS")
                        {
                            // QueryParam = @$"first: {orderCount}, query: ""financial_status:'pending' AND fulfillment_status:'unfulfilled' AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";
                            QueryParam = @$"first: {orderCount}, query: ""financial_status:pending AND fulfillment_status:unfulfilled AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";
                        }
                        else if (service.ToUpper() == "INVOICES")
                        {
                            QueryParam = @$"first: {orderCount}, query: ""financial_status:'paid' AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";   // AND fulfillment_status:'unfulfilled' 
                            //QueryParam = @$"first: {orderCount}, query: ""updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";   // AND fulfillment_status:'unfulfilled' 
                        }
                        else if (service.ToUpper() == "REFUNDED")
                        {
                            QueryParam = @$"first: {orderCount}, query: ""financial_status:'refunded' AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";
                        }
                        else if (service.ToUpper() == "RETURNED")
                        {
                            QueryParam = @$"first: {orderCount}, query: ""financial_status:'voided' AND fulfillment_status:'fulfilled' AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";
                        }
                        else if (service.ToUpper() == "PARTIALLY_REFUNDED")
                        {
                            QueryParam = @$"first: {orderCount}, query: ""financial_status:'partially_refunded' AND fulfillment_status:'fulfilled' AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";
                        }
                        else if (service.ToUpper() == "CANCELLED")
                        {
                            QueryParam = @$"first: {orderCount}, query: ""status:'cancrelled' AND updated_at:>'{createdAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"")}";
                        }

                        var orderGQ = GraphQuery.Order(QueryParam);
                        var response = await GraphAPI.QueryAsync(orderGQ) ?? null;

                        if (response != null && response.Errors == null)
                        {
                            var data = JsonConvert.DeserializeObject<OrderResponce>(JsonConvert.SerializeObject(response.Data));
                            var orderList = data.Orders.Edges.Select(e => e.Node).ToList();


                            if (service.ToUpper() == "INVOICES")
                            {
                                orderList.RemoveAll(o => o.DisplayFulfillmentStatus != null && o.DisplayFulfillmentStatus?.ToLower() == "fulfilled");
                            }

                            if (service.ToUpper() == "REFUNDED")
                            {
                                orderList.RemoveAll(o => o.CancelledAt != null);
                            }

                            if (service.ToUpper() == "ORDERS")
                            {
                                orderList.Where(o => o.CancelledAt != null).ToList().ForEach(o=>o.IsCancelled = true);
                            }

                            foreach (var order in orderList)
                            {
                                try
                                {

                                    long.TryParse(order.Id.Split('/').Last(), out long OrderId);
                                    order.OrderId = OrderId;
                                    order.LineItemList.AddRange(order.LineItems.Edges.Where(n => n.Node.CurrentQuantity > 0).Select(i => i.Node));
                                    
                                    var orderFilter = Builders<BsonDocument>.Filter.Eq("order_id", order.OrderId);
                                    var orderResult = await OrderCollection.Find(orderFilter).ToListAsync();
                                    var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                                    var existingDocument = JsonConvert.DeserializeObject<List<OMNI_Dashboard.ApiControllers.OrderForListing>>(JsonConvert.SerializeObject(orderObj))?.FirstOrDefault();

                                    var shopifyOrderFilter = Builders<BsonDocument>.Filter.Eq("name", order.Name);
                                    var originalSaleOrderResult = await shopifyOrderCollection.Find(shopifyOrderFilter).ToListAsync();
                                    var originalSaleOrderObject = originalSaleOrderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                                    var originalSaleOrder = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(originalSaleOrderObject))?.FirstOrDefault();

                                    if (existingDocument == null)
                                    {
                                        if (order.PaymentGatewayNames == null || order.PaymentGatewayNames.Count == 0)
                                        {
                                            order.PaymentGatewayNames = new List<string>() { "Cash on Delivery (COD)" };
                                        }
                                        if (service.ToUpper() == "REFUNDED" || service.ToUpper() == "PARTIALLY_REFUNDED" || service.ToUpper() == "RETURNED" || service.ToUpper() == "PARTIALLY_REFUNDED")
                                        {
                                            if (originalSaleOrder != null && originalSaleOrder.posted)
                                            {
                                                order.Refunds.ForEach(refund =>
                                                {
                                                    refund.TransactionList.AddRange(refund.Transactions.Edges.Select(e => e.Node));
                                                    refund.Transactions = null;

                                                    if (service.ToUpper() == "REFUNDED" || service.ToUpper() == "PARTIALLY_REFUNDED" || service.ToUpper() == "RETURNED")
                                                    {
                                                        foreach (var item in refund.RefundLineItems.Edges)
                                                        {
                                                            item.Node.IsProcessed = false;
                                                        }
                                                    }
                                                });

                                                if (service.ToUpper() == "PARTIALLY_REFUNDED")
                                                {
                                                    var existingrefunds = originalSaleOrder?.Refunds ?? [];
                                                    if (existingrefunds.Count > 0)
                                                    {
                                                        foreach (var existingrefund in existingrefunds)
                                                        {
                                                            order.Refunds.RemoveAll(r => r.Id == existingrefund.Id);
                                                        }
                                                    }
                                                }

                                                order.Fulfillments.ForEach(fulfillment =>
                                                {
                                                    fulfillment.FulfillmentLineItemList.AddRange(fulfillment.FulfillmentLineItems.Edges.Select(f => f.Node));
                                                    fulfillment.FulfillmentLineItems = null;
                                                });

                                                if (order.PaymentGatewayNames == null || order.PaymentGatewayNames.Count == 0)
                                                {
                                                    order.PaymentGatewayNames = new List<string>() { "Cash on Delivery (COD)" };
                                                }
                                            }
                                        }

                                        var orderDocument = BsonDocument.Parse(JsonConvert.SerializeObject(order, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                                        var destinationAddress = order.ShippingAddress?.Address1 + order.ShippingAddress?.Address2;

                                        orderDocument.Add(new BsonElement("order_is_verified", false));
                                        orderDocument.Add(new BsonElement("dispatched", false));
                                        orderDocument.Add(new BsonElement("assigned_store_no", -1));
                                        orderDocument.Add(new BsonElement("assigned_store_sid", 0));

                                        orderDocument.Add(new BsonElement("courier", new BsonDocument { { "courier_name", "" }, { "cn_number", "" }, { "destination_city", order.ShippingAddress?.City }, { "destination_address", destinationAddress } }));
                                        orderDocument.Add(new BsonElement("courier_error", new BsonDocument { { "courier_error", false }, { "short_inventory", false }, { "error_message", "" } }));
                                        orderDocument.Add(new BsonElement("courier_retry", DateTime.Now.AddMinutes(5)));
                                        orderDocument.Add(new BsonElement("is_courier_assigned", false));
                                        orderDocument.Add(new BsonElement("accepted_by_store", ""));

                                        orderDocument.Add(new BsonElement("order_error_state", false));
                                        orderDocument.Add(new BsonElement("order_source", "Shopify"));
                                        orderDocument.Add(new BsonElement("printed", 0));
                                        orderDocument.Add(new BsonElement("fulfullment_sent", false));
                                        orderDocument.Add(new BsonElement("invoice", new BsonDocument { { "created", 0 }, { "invoice_Id", 0 }, { "invoice_synced", 0 }, { "invoice_synced_dateTime", DateTime.Now }, { "synced_invoice_sid", 0 } }));
                                       
                                        orderDocument["has_error"] = new BsonBoolean(false);
                                        orderDocument["posted"] = new BsonBoolean(false);
                                        orderDocument["isCancelled"] = new BsonBoolean(false);
                                        orderDocument.Add(new BsonElement("retailProSid", -1));

                                        var cityProjection = Builders<BsonDocument>.Projection.Include("name").Exclude("_id");
                                        var cityResult = await CityCollection.Find($@"{{country_name:{{$in:[""United Arab Emirates""]}}}}").Project(cityProjection).ToListAsync();
                                        var cityResultObject = cityResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                                        var CityList = JsonConvert.DeserializeObject<List<CityModel>>(JsonConvert.SerializeObject(cityResultObject))?.Select(s => s.CityName).ToList() ?? [];

                                        var possibleCityList = CityList.Where(c => Soundex.For(c) == Soundex.For(order.ShippingAddress?.City ?? "")).ToList();
                                        var possibleDestinationCity = possibleCityList.FirstOrDefault(c => LevenshteinDistance.CalculateSimilarity(c, order.ShippingAddress?.City ?? "") > 80);

                                        if (string.IsNullOrEmpty(possibleDestinationCity))
                                        {
                                            orderDocument.Add(new BsonElement("order_tracking_status", new BsonDocument { { "status_code", 3030 }, { "status", "INVALID CITY" }, { "status_updated_at", DateTime.Now } }));
                                            orderDocument.Add(new BsonElement("order_tracking_history", new BsonArray { new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } }, new BsonDocument { { "status_code", 3030 }, { "status", "INVALID CITY" }, { "order_error_state", true }, { "status_updated_at", DateTime.Now } } }));
                                        }
                                        else
                                        {
                                            if (GlobalVariables.CourierServiceisAvailable)
                                            {
                                                orderDocument.Add(new BsonElement("order_tracking_status", new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } }));
                                                orderDocument.Add(new BsonElement("order_tracking_history", new BsonArray { new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } } }));
                                            }
                                            else
                                            {
                                                orderDocument.Add(new BsonElement("order_tracking_status", new BsonDocument { { "status_code", 2000 }, { "status", "APPROVED" }, { "status_updated_at", DateTime.Now } }));
                                                orderDocument.Add(new BsonElement("order_tracking_history", new BsonArray { new BsonDocument { { "status_code", 1000 }, { "status", "NEW" }, { "status_updated_at", DateTime.Now } }, new BsonDocument { { "status_code", 2000 }, { "status", "APPROVED" }, { "status_updated_at", DateTime.Now } } }));
                                            }
                                        }

                                        if (order.ShippingAddress == null || order.BillingAddress == null)
                                        {
                                            orderDocument["error_message"] = "Shipping Address / Billing Address Not found ";
                                            orderDocument["has_error"] = new BsonBoolean(true);
                                            orderDocument["posted"] = new BsonBoolean(false);
                                            orderDocument["isCancelled"] = new BsonBoolean(false);
                                        }
                                        else
                                        {
                                            if (originalSaleOrder != null && originalSaleOrder.posted)
                                            {
                                                orderDocument["posted"] = new BsonBoolean(false);
                                                orderDocument["has_error"] = new BsonBoolean(false);
                                                orderDocument["isCancelled"] = new BsonBoolean(false);
                                            }
                                            else
                                            {
                                                if (service.ToUpper() == "REFUNDED" || service.ToUpper() == "PARTIALLY_REFUNDED" || service.ToUpper() == "RETURNED")
                                                {
                                                    // filter on order_id and retailProSid to be -1 only then update
                                                    var shopifyOrderFilter2 = Builders<BsonDocument>.Filter.Eq("order_id", order.OrderId) &
                                                                        Builders<BsonDocument>.Filter.Eq("retailProSid", -1);

                                                    var shopifyOrderUpdate = Builders<BsonDocument>.Update
                                                        .Set("status", "Refunded")
                                                        .Set("posted", true);

                                                    await shopifyOrderCollection.UpdateOneAsync(shopifyOrderFilter2, shopifyOrderUpdate);

                                                    //orderDocument["status"] = "Refunded";
                                                    orderDocument["posted"] = new BsonBoolean(true);
                                                    orderDocument["has_error"] = new BsonBoolean(false);
                                                    orderDocument["isCancelled"] = new BsonBoolean(false);
                                                }
                                            }
                                        }

                                        //var existingDocument = OrderCollection.Find(orderFilter).FirstOrDefault();

                                        await OrderCollection.InsertOneAsync(orderDocument);
                                    }
                                    else
                                    {
                                        if (service.ToUpper() == "REFUNDED" || service.ToUpper() == "PARTIALLY_REFUNDED" || service.ToUpper() == "RETURNED")
                                        {
                                            // ===========================================================================================

                                            var existingRefunds = existingDocument.Refunds ?? [];

                                            order.Refunds.ForEach(refund =>
                                            {
                                                var existingRefund = existingRefunds.Where(r => r.Id == refund.Id).FirstOrDefault();

                                                if (existingRefund != null)
                                                {
                                                    existingRefund = null;
                                                }
                                                else
                                                {
                                                    refund.TransactionList.AddRange(refund.Transactions.Edges.Select(e => e.Node));
                                                    refund.Transactions = null;

                                                    foreach (var item in refund.RefundLineItems.Edges)
                                                    {
                                                        item.Node.IsProcessed = false;
                                                    }
                                                }
                                            });

                                            var existingrefunds = originalSaleOrder?.Refunds ?? [];
                                            if (existingrefunds.Count > 0)
                                            {
                                                foreach (var existingrefund in existingrefunds)
                                                {
                                                    order.Refunds.RemoveAll(r => r.Id == existingrefund.Id);
                                                }
                                            }
                                        }

                                        order.Fulfillments.ForEach(fulfillment =>
                                        {
                                            fulfillment.FulfillmentLineItemList.AddRange(fulfillment.FulfillmentLineItems.Edges.Select(f => f.Node));
                                            fulfillment.FulfillmentLineItems = null;
                                        });

                                        //if (order.PaymentGatewayNames == null || order.PaymentGatewayNames.Count == 0)
                                        //{
                                        //    order.PaymentGatewayNames = new List<string>() { "Cash on Delivery (COD)" };
                                        //}

                                        var orderDocument = BsonDocument.Parse(JsonConvert.SerializeObject(order, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                                        orderDocument.Add(new BsonElement("order_is_verified", false));
                                        orderDocument.Remove("events");

                                        var updates = new List<UpdateDefinition<BsonDocument>>();

                                        foreach (var element in orderDocument.Elements)
                                        {
                                            updates.Add(Builders<BsonDocument>.Update.Set(element.Name, element.Value));
                                        }


                                        if (!string.IsNullOrEmpty(order.ShippingAddress?.City))
                                        {
                                            updates.Add(
                                                Builders<BsonDocument>.Update.Set(
                                                    "courier.destination_city",
                                                    order.ShippingAddress.City
                                                )
                                            );
                                        }

                                        var destinationAddress = order.ShippingAddress?.Address1 + order.ShippingAddress?.Address2;
                                        if (!string.IsNullOrEmpty(destinationAddress))
                                        {
                                            updates.Add(
                                                Builders<BsonDocument>.Update.Set(
                                                    "courier.destination_address",
                                                    destinationAddress
                                                )
                                            );
                                        }


                                        var combinedUpdate = Builders<BsonDocument>.Update.Combine(updates);

                                        await OrderCollection.UpdateOneAsync(
                                            orderFilter,
                                            combinedUpdate,
                                            new UpdateOptions { IsUpsert = true }
                                        );

                                    }

                                    if (service.ToUpper() == "CANCELLED")
                                    {
                                        // filter on order_id and retailProSid to be -1 only then update
                                        var shopifyOrderFilter2 = Builders<BsonDocument>.Filter.Eq("order_id", order.OrderId) &
                                                                Builders<BsonDocument>.Filter.Eq("retailProSid", -1);

                                        // update order status to cancelled in shopify_orders collection
                                        var shopifyOrderUpdate = Builders<BsonDocument>.Update
                                            .Set("status", "Cancelled")
                                            .Set("cancelled_at", order.CancelledAt)
                                            .Set("cancel_reason", order.CancelReason);

                                        await shopifyOrderCollection.UpdateOneAsync(shopifyOrderFilter2, shopifyOrderUpdate);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    BsonDocument document = new BsonDocument();
                                    document["service"] = service;
                                    document["created_at"] = DateTime.Now;
                                    document["document_no"] = order.Name;
                                    document["exception_message"] = ex.Message;
                                    document["exception_source"] = "Shopify GraphQL Fetch Orders";
                                    document["exception_stack_trace"] = "";
                                    document["seen"] = false;
                                    await exceptionCollection.InsertOneAsync(document);
                                }
                            }

                            hasNextRecord = data.Orders.PageInfo.HasNextPage;
                            endCursor = data.Orders.PageInfo.EndCursor;
                        }
                        else if (response?.Errors != null)
                        {
                            foreach (var err in response.Errors)
                            {
                                BsonDocument document = new BsonDocument();
                                document["service"] = service;
                                document["created_at"] = DateTime.Now;
                                document["exception_message"] = err.Message;
                                document["exception_source"] = "Shopify GraphQL Fetch Orders";
                                document["exception_stack_trace"] = "";
                                document["seen"] = false;
                                await exceptionCollection.InsertOneAsync(document);
                            }
                        }

                        var exceptionUpdate = Builders<BsonDocument>.Update
                                    .Set("last_updated_at", DateTime.Now);
                        await OrderLogCollection.UpdateOneAsync(filter, exceptionUpdate);
                    }
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

        private async Task<bool> GetOrderEvents()
        {
            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                var shopifyConfig = GlobalVariables.ShopifyConfig;
                var OrderEventsInfoLogCollection = mongoDB.GetCollection<BsonDocument>("order_event_info_log");
                var shopifyOrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");

                if (shopifyConfig != null)
                {
                    var filter = @$"{{type:'events'}}";
                    var logResult = await OrderEventsInfoLogCollection.Find(filter).ToListAsync();
                    var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                    DateTimeOffset LastUpdated = DateTimeOffset.Now;
                    var startTime = DateTime.Now;
                    if (log == null)
                    {
                        BsonDocument document = new()
                        {
                            ["type"] = "events",
                            ["created_at"] = DateTime.Now,
                            ["last_updated_at"] = DateTime.Now,
                        };
                        await OrderEventsInfoLogCollection.InsertOneAsync(document);
                        LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        _ = DateTimeOffset.TryParse(log["last_updated_at"]?.ToString(), out DateTimeOffset LastUpdatedAt);

                        if (LastUpdatedAt != DateTimeOffset.MinValue)
                        {
                            LastUpdated = LastUpdatedAt;
                        }
                    }

                    var ShopifyEventsResult = ShopifyEvents.GetOrderEventsSequentially(LastUpdated).Result;
                    var uniqueOrderIds = ShopifyEventsResult.Select(e => e.SubjectId).Distinct().ToList();

                    foreach (var uniqueOrderId in uniqueOrderIds)
                    {
                        var eventsForOrder = ShopifyEventsResult.Where(e => e.SubjectId == uniqueOrderId).ToList();
                        eventsForOrder.ForEach(s => s.Action = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.Action.Replace("_", " ").ToLower()));
                        _ = long.TryParse(uniqueOrderId.Split('/').Last(), out long OrderId);
                        var shopifyOrderFilter = Builders<BsonDocument>.Filter.Eq("order_id", OrderId);
                        var shopifyOrderUpdate = Builders<BsonDocument>.Update.PushEach("events", eventsForOrder);
                        await shopifyOrderCollection.UpdateOneAsync(shopifyOrderFilter, shopifyOrderUpdate);

                        await DeduplicateOrderEventsAsync(shopifyOrderCollection, shopifyOrderFilter);
                    }
                }

                var exceptionUpdate = Builders<BsonDocument>.Update.Set("last_updated_at", DateTime.Now);
                await OrderEventsInfoLogCollection.UpdateOneAsync(@$"{{type:'events'}}", exceptionUpdate);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        
        }

        public async static Task DeduplicateOrderEventsAsync( IMongoCollection<BsonDocument> collection,  FilterDefinition<BsonDocument> shopifyOrderFilter)
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$set", new BsonDocument("events",
                    new BsonDocument("$reduce", new BsonDocument
                    {
                        { "input", "$events" },
                        { "initialValue", new BsonArray() },
                        {
                            "in",
                            new BsonDocument("$cond", new BsonArray
                            {
                                new BsonDocument("$in", new BsonArray
                                {
                                    "$$this._id",
                                    new BsonDocument("$map", new BsonDocument
                                    {
                                        { "input", "$$value" },
                                        { "as", "e" },
                                        { "in", "$$e._id" }
                                    })
                                }),
                                "$$value",
                                new BsonDocument("$concatArrays",
                                    new BsonArray { "$$value", new BsonArray { "$$this" } })
                            })
                        }
                    })
                ))
            };


            await collection.UpdateOneAsync(
                shopifyOrderFilter,
                PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline)
            );
        }



    }
}