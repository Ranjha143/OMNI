using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;
using Quartz;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text;
using GraphQL;
using System.Security.Policy;

namespace Shopify
{
    [DisallowConcurrentExecution]
    internal class CancelShopifyOrder : IJob
    {
        private readonly string MongoConnectionString = "";
        private readonly string MongoDatabase = "";
        readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public CancelShopifyOrder()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;

            MongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoDatabase = GlobalVariables.MongoDatabase;

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

            MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
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
            var workerTask = Task.Factory.StartNew(() => CancelOrders().Wait());
            workerTask.Wait();
        }

        private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs? e)
        {

        }
        private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs? e)
        {
            GlobalVariables.ShopifyOrderWorker = false;
        }
        private async Task<bool> CancelOrders()
        {
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);

            try
            {

                var shopifyConfig = GlobalVariables.ShopifyConfig;
                if (shopifyConfig != null)
                {
                    var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
                    var orderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                    var logCollection = mongoDB.GetCollection<BsonDocument>("shopify_order_log");


                    var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");
                    var CityCollection = mongoDB.GetCollection<BsonDocument>("cities");

                    var filter = "{type:'Cancel Shopify Order'}";
                    var logResult = await logCollection.Find(filter).ToListAsync();
                    var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                    DateTimeOffset LastUpdated = DateTimeOffset.Now; 
                    var startTime = DateTime.Now;
                    if (log == null)
                    {
                        BsonDocument document = new()
                        {
                            ["type"] = "Cancel Shopify Order",
                            ["created_at"] = DateTime.Now,
                            ["last_updated_at"] = DateTime.Now,
                            ["total_orders"] = 0,
                            ["processed"] = 0,
                            ["failed"] = 0
                        };
                        await logCollection.InsertOneAsync(document);
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

                    // get fulfilled Orders
                    var filterQuery = @"{
                                          ""status"": ""cancelled"",
                                          ""$or"": [
                                            { ""cancellation_sent"": { ""$exists"": false } },
                                            { ""cancellation_sent"": false }
                                          ]
                                        }";

                    var orderResult = await orderCollection.Find(filterQuery).ToListAsync();
                    var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var orderList = JsonConvert.DeserializeObject<List<OrderNode>>(JsonConvert.SerializeObject(orderObj))?.ToList() ?? [];

                    foreach (var order in orderList)
                    {
                        try
                        {
                            var res = await CancelAndRestockFulfillmentAsync(order.Id);
                            if (res)
                            {
                                res = await CancelOrderAsync(order.Id);
                                if (res)
                                {
                                    // mark order as cancellatoion sent = true
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
                var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

                BsonDocument document = new BsonDocument();
                document["service"] = "Cancel Shopify Orders";
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }
            return true;
        }

        // 2️⃣ Cancel Fulfillment and Restock Items
        public async Task<bool> CancelAndRestockFulfillmentAsync(string orderId)
        {
            var fulfillmentOrderIds = await GetFulfillmentOrderIdsAsync(orderId);
            var mutation = new GraphQLRequest
            {
                Query = @"
                mutation fulfillmentOrderCancel($fulfillmentOrderId: ID!) {
                  fulfillmentOrderCancel(id: $fulfillmentOrderId) {
                    fulfillmentOrder {
                      id
                      status
                    }
                    userErrors {
                      field
                      message
                    }
                  }
                }",
                Variables = new { fulfillmentOrderIds }
            };

            var response = await GraphAPI.SendMutation(mutation);
            if (response.Errors != null)
            {
                throw new Exception("GraphQL mutation errors: " + JsonConvert.SerializeObject(response.Errors));
            }
            return true;
        }

        public async Task<List<string>> GetFulfillmentOrderIdsAsync(string orderId)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query GetFulfillmentOrderIds($orderId: ID!) {
                    order(id: $orderId) {
                        id
                        fulfillmentOrders(first: 10) {
                            edges {
                                node {
                                    id
                                    lineItems(first: 10) {
                                        edges {
                                            node {
                                                id
                                                quantity
                                                fulfillmentOrder {
                                                    id
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }",
                Variables = new
                {
                    orderId
                }
            };

            var response = await GraphAPI.QueryAsync(query);

            if (response != null && response.Errors != null)
            {
                throw new Exception("GraphQL query errors: " + JsonConvert.SerializeObject(response.Errors));
            }

            var fulfillmentOrderIds = new List<string>();

            var data = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(response.Data));

            var edges = data["order"]["fulfillmentOrders"]["edges"];

            foreach (var edge in edges)
            {
                var lineItems = edge["node"]["lineItems"]["edges"];
                foreach (var lineItem in lineItems)
                {
                    var fulfillmentOrderId = lineItem["node"]["fulfillmentOrder"]["id"].ToString();
                    fulfillmentOrderIds.Add(fulfillmentOrderId);
                }
            }

            return fulfillmentOrderIds;
        }
        
        // 3️⃣ Cancel the Order
        public async Task<bool> CancelOrderAsync(string orderId)
        {
            var mutation = new GraphQLRequest
            {
                Query = @"
                mutation orderCancel($orderId: ID!) {
                  orderCancel(id: $orderId) {
                    order {
                      id
                      status
                    }
                    userErrors {
                      field
                      message
                    }
                  }
                }",
                Variables = new { orderId }
            };

            var response = await GraphAPI.SendMutation(mutation);

            if (response.Errors != null)
            {
                throw new Exception("GraphQL mutation errors: " + JsonConvert.SerializeObject(response.Errors));
            }
            return true;
        }

    }
}
