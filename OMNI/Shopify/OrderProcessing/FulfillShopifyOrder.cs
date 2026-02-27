using GraphQL;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMNI_Dashboard.ApiControllers;
using PluginManager;
using Quartz;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace Shopify
{
    [DisallowConcurrentExecution]
    internal class FulfillShopifyOrder : IJob
    {
        private readonly string MongoConnectionString = "";
        private readonly string MongoDatabase = "";
        readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public FulfillShopifyOrder()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;

            MongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoDatabase = GlobalVariables.MongoDatabase;

        }

        public async Task Execute(IJobExecutionContext context)
        {
            //var workerTask = Task.Factory.StartNew(() => LoadConfigurations().Wait());
            //Task.WaitAll(workerTask);

            if (!GlobalVariables.ShopifyFulfillOrderWorker)
            {
                GlobalVariables.ShopifyFulfillOrderWorker = true;
                await Task.Delay(0);
                threadWorker.RunWorkerAsync();
            }
        }

        //private async Task<bool> LoadConfigurations()
        //{

        //    MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
        //    var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
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
            var workerTask = Task.Factory.StartNew(() => FulfillOrder().Wait());
            workerTask.Wait();
        }

        private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs? e)
        {

        }
        private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs? e)
        {
            GlobalVariables.ShopifyFulfillOrderWorker = false;
        }
        private async Task<bool> FulfillOrder()
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

                    var filter = "{type:'Fulfill Shopify Order'}";
                    var logResult = await logCollection.Find(filter).ToListAsync();
                    var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                    DateTimeOffset LastUpdated = DateTimeOffset.Now; 
                    var startTime = DateTime.Now;
                    if (log == null)
                    {
                        BsonDocument document = new()
                        {
                            ["type"] = "Fulfill Shopify Order",
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
                    // var filterQuery = "{fulfullment_sent:false, status:'fulfilled'}";

                    var filterQuery = "{fulfullment_sent:false, status:'fulfilled', dispatched:true, posted:true}";
                    var orderResult = await orderCollection.Find(filterQuery).ToListAsync();
                    var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var orderList = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderObj))?.ToList() ?? [];

                    foreach (var order in orderList)
                    {
                        try
                        {
                            var res = await FulfillOrderAsync(order);
                            if (res)
                            {
                                var updateFilter = "{order_id:" + order.OrderId + "}";
                                var update = Builders<BsonDocument>.Update.Set("posted", true).Set("fulfullment_sent",true);
                                orderCollection.FindOneAndUpdate(filterQuery, update);

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
                document["service"] = "Fulfill Shopify Orders";
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }
            return true;
        }

        public async Task<FulfilmentOrderInfo> GetFulfillmentOrderIdsAsync(string orderId)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                query GetFulfillmentOrdersForOrder($orderId: ID!) {
                  order(id: $orderId) {
                    fulfillmentOrders(first: 50) {
                      edges {
                        node {
                          id
                          status
                          lineItems(first: 50) {
                            edges {
                              node {
                                id  # FulfillmentOrderLineItem ID
                                lineItem {
                                  id
                                  name
                                  sku
                                }
                                totalQuantity
                                remainingQuantity
                                variant {
                                  id
                                  title
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
                    orderId = orderId
                }
            };

            var response = await GraphAPI.QueryAsync(query);

            if (response != null && response.Errors != null)
            {
                throw new Exception("GraphQL query errors: " + JsonConvert.SerializeObject(response.Errors));
            }

            FulfilmentOrderInfo fulfilmentOrderInfo = new FulfilmentOrderInfo();

             var data = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(response.Data));

            var edges = data["order"]["fulfillmentOrders"]["edges"];

            foreach (var edge in edges)
            {
                fulfilmentOrderInfo.fulfilmentOrderIdString = edge["node"]["id"].ToString();
                var lineItems = edge["node"]["lineItems"]["edges"];
                foreach (var lineItem in lineItems)
                {
                    var fulfillmentOrderItemId = lineItem["node"]["id"].ToString();


                    var itemSKU = lineItem["node"]["lineItem"]["sku"].ToString();

                    fulfilmentOrderInfo.fulfillmentOrderItemIds.Add( new fulfillmentOrderItems {fulfillmentOrderItemId = fulfillmentOrderItemId, itemSku = itemSKU });
                }
            }

          
            return fulfilmentOrderInfo;
        }

        public async Task<bool> FulfillOrderAsync(OrderForListing order)
        {
            var fulfillmentOrderInfo = await GetFulfillmentOrderIdsAsync(order.Id);

            var itemList = order.LineItemList;

            var fulfillmentOrderLineItems = fulfillmentOrderInfo.fulfillmentOrderItemIds
                .Select(item => new
                {
                    id = item.fulfillmentOrderItemId,
                    quantity = itemList.Where(i=>i.Sku == item.itemSku).Select(s=>s.Quantity).FirstOrDefault()
                })
                .ToList();

            bool notifyCustomer = true;
            var mutation = new GraphQLRequest
            {
                Query = @"
                    mutation fulfillmentCreateV2($fulfillment: FulfillmentV2Input!) {
                        fulfillmentCreateV2(fulfillment: $fulfillment) {
                            fulfillment {
                                id
                                status
                                trackingInfo {
                                    company
                                    number
                                    url
                                }
                            }
                            userErrors {
                                field
                                message
                            }
                        }
                    }",
                Variables = new
                {
                    fulfillment = new
                    {
                        notifyCustomer,
                        trackingInfo = new
                        {
                            company = order.Courier.CourierName,
                            number = order.Courier.CnNumber
                        },
                        lineItemsByFulfillmentOrder = new[]
                                    {
                            new
                            {
                                fulfillmentOrderId = fulfillmentOrderInfo.fulfilmentOrderIdString,
                                fulfillmentOrderLineItems
                            }
                        }
                    }
                }
            };



            var response = await GraphAPI.SendMutation(mutation);
            File.AppendAllText("fulfilment.txt", JsonConvert.SerializeObject(response));
            File.AppendAllText("fulfilment.txt", Environment.NewLine);
            File.AppendAllText("fulfilment.txt", Environment.NewLine);
            File.AppendAllText("fulfilment.txt", Environment.NewLine);

            if (response.Errors != null)
            {
                throw new Exception("GraphQL mutation errors: " + JsonConvert.SerializeObject(response.Errors));
            }
            else
            {
                return true;
            }
        }

    }


    public class FulfilmentOrderInfo
    {
        public string fulfilmentOrderIdString { get; set; }
        public List<fulfillmentOrderItems> fulfillmentOrderItemIds { get; set; } = [];
    }

    public class fulfillmentOrderItems
    { 
        public string fulfillmentOrderItemId { get; set; }
        public string itemSku { get; set; }

        


    }
}
