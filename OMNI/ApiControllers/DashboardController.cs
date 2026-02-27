using Amazon.Runtime.Internal;
using GraphQL;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMNI;
using OMNI.ApiCalls;
using OMNI.Shopify.DataModels;
using PluginManager;
using RetailPro_V22;
//using RetailPro2_X;
using RetailPro2_X.BL;
using Shopify;
//using ShopifySharp;
//using ShopifySharp.GraphQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceProcess;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using TimeZoneConverter;

namespace OMNI_Dashboard.ApiControllers
{
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private IMongoCollection<BsonDocument> logCollection;
        private IMongoCollection<BsonDocument> exceptionCollection;
        private IMongoCollection<UserInfo> userCollection;
        private readonly MongoClient mongoDbClient = new(Globals.MongoConnectionString);

        public DashboardController()
        {
            var mongoDB = mongoDbClient.GetDatabase(Globals.MongoDatabase);
            userCollection = mongoDB.GetCollection<UserInfo>("users");
        }

        [HttpGet]
        [Route("api/v1/service_status")]
        public async Task<IActionResult> ServiceStatus()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                var serviceStatusInfo = new ServiceStatusInfo();

                IMongoCollection<BsonDocument> retailproInventoryLog = mongoDB.GetCollection<BsonDocument>("retailpro_inventory_log");
                IMongoCollection<BsonDocument> shopifyOrderLog = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");
                IMongoCollection<BsonDocument> shopifyInventoryLog = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
                IMongoCollection<BsonDocument> Services = mongoDB.GetCollection<BsonDocument>("omni_services");

                var filter = "{}";
                var rpResult = await retailproInventoryLog.Find(filter).ToListAsync();
                var rpResultObject = rpResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var rProLog = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(rpResultObject))?.FirstOrDefault();

                var shopifyResult = await shopifyOrderLog.Find("{type:'Orders'}").ToListAsync();
                var shopifyResultObject = shopifyResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var shopifyLog = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(shopifyResultObject))?.FirstOrDefault();

                var servicesResult = await Services.Find(filter).ToListAsync();
                var servicesResultObject = servicesResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var serviceInfo = JsonConvert.DeserializeObject<List<ServiceStatus>>(JsonConvert.SerializeObject(servicesResultObject))?.ToList();

                serviceStatusInfo.InventoryService = serviceInfo.Where(s => s.service.Equals("Inventory"))?.FirstOrDefault()?.enabled ?? false;
                serviceStatusInfo.OrderService = serviceInfo.Where(s => s.service.Equals("Order"))?.FirstOrDefault()?.enabled ?? false;
                serviceStatusInfo.InvnSrvLastRunAt = DateTimeOffset.Parse(rProLog["last_updated_at"]?.ToString());
                serviceStatusInfo.OrderSrvLastRunAt = DateTimeOffset.Parse(shopifyLog["last_updated_at"]?.ToString());
                serviceStatusInfo.InvTimeElapsed = rProLog["total_time_elapsed"].ToString();
                serviceStatusInfo.Success = true;

                return StatusCode((int)HttpStatusCode.OK, new { serviceStatusInfo });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }

        [HttpGet]
        [Route("api/v1/order_state")]
        public async Task<IActionResult> OrderState(string fromDate, string toDate, string userId)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            OrderState orderStatusInfo = new OrderState();
            //DateTime compareFromDate = DateTime.Now.AddDays(-3).Date, compareToDate = DateTime.Now;
            try
            {
                IMongoCollection<BsonDocument> configCollection = mongoDB.GetCollection<BsonDocument>("plugin_config");

                var userInfo = await userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

                if (userInfo.role == "Super Admin" || userInfo.role == "Admin")
                {
                    // Shopify Order Count
                    long shopifyOrderCount = 0;
                    var filter = $@"{{""platform"":""Shopify""}}";
                    var configResult = await configCollection.Find(filter).ToListAsync();
                    var configObject = configResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var configuration = JsonConvert.DeserializeObject<List<ConfigurationVM>>(JsonConvert.SerializeObject(configObject)).FirstOrDefault();

                    //var countResult = await GraphAPI.QueryAsync(GraphQuery.productCount());
                    //var data = JsonConvert.DeserializeObject<ProductCountInfo>(JsonConvert.SerializeObject(countResult.Data));
                    //shopifyOrderCount = data.ProductsCount.Count;

                    IMongoCollection<BsonDocument> shopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                    var orderFilter = "{}"; //"{createdAt: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";

                    var downloadedOrders = await shopifyOrders.CountDocumentsAsync(orderFilter);

                    var orderProcessedFilter = "{}"; //""{xml_generated:true, createdAt: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";
                    var processedOrders = await shopifyOrders.CountDocumentsAsync(orderProcessedFilter);

                    var orderErrorFilter = "{}"; //" "{has_error:true, createdAt: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";
                    var errorOrders = await shopifyOrders.CountDocumentsAsync(orderErrorFilter);

                    var orderSyncedFilter = "{}"; //" "{RetailProSID:{$gt: -1}, createdAt: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";
                    var SyncedOrders = await shopifyOrders.CountDocumentsAsync(orderSyncedFilter);

                    orderStatusInfo = new OrderState
                    {
                        ShopifyWebOrders = downloadedOrders,
                        ShopifyDownloadedOrders = downloadedOrders,
                        RetailProInvoicesProcessed = processedOrders,
                        OrdersInErrorState = errorOrders,
                        RetailProInvoicesFound = SyncedOrders
                    };
                }
                else if (userInfo.role == "Staff")
                {
                    // Shopify Order Count
                    //long shopifyOrderCount = 0;
                    var filter = $@"{{""platform"":""Shopify""}}";
                    var configResult = await configCollection.Find(filter).ToListAsync();
                    var configObject = configResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var configuration = JsonConvert.DeserializeObject<List<ConfigurationVM>>(JsonConvert.SerializeObject(configObject)).FirstOrDefault();

                    //var countResult = await GraphAPI.QueryAsync(GraphQuery.productCount());
                    //var data = JsonConvert.DeserializeObject<ProductCountInfo>(JsonConvert.SerializeObject(countResult.Data));
                    //shopifyOrderCount = data.ProductsCount.Count;
                    // assigned_store_sid:{userInfo.assigned_store}

                    IMongoCollection<BsonDocument> shopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                    var orderFilter = "{createdAt: { $gte:'" + fromDate + "' ,  $lte:'" + toDate + "' } }";
                    var downloadedOrders = await shopifyOrders.CountDocumentsAsync(orderFilter);

                    var orderProcessedFilter = "{assigned_store_sid:'" + userInfo.assigned_store + "', createdAt: { $gte:'" + fromDate + "' ,  $lte:'" + toDate + "' } }";
                    var processedOrders = await shopifyOrders.CountDocumentsAsync(orderProcessedFilter);

                    var orderErrorFilter = "{assigned_store_sid:'" + userInfo.assigned_store + "', has_error:true, createdAt: { $gte:'" + fromDate + "' ,  $lte:'" + toDate + "' } }";
                    var errorOrders = await shopifyOrders.CountDocumentsAsync(orderErrorFilter);

                    var orderSyncedFilter = "{assigned_store_sid:'" + userInfo.assigned_store + "', RetailProSID:{$gt: -1}, createdAt: { $gte:'" + fromDate + "' ,  $lte:'" + toDate + "' } }";
                    var SyncedOrders = await shopifyOrders.CountDocumentsAsync(orderSyncedFilter);

                    orderStatusInfo = new OrderState
                    {
                        ShopifyWebOrders = downloadedOrders,
                        ShopifyDownloadedOrders = downloadedOrders,
                        RetailProInvoicesProcessed = processedOrders,
                        OrdersInErrorState = errorOrders,
                        RetailProInvoicesFound = SyncedOrders
                    };
                }
                return StatusCode((int)HttpStatusCode.OK, new { orderStatusInfo });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }


        [HttpGet]
        [Route("api/v1/order_counts")]
        public async Task<IActionResult> OrderCounts(string fromDate, string toDate, string userId)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            IMongoCollection<UserInfo> userCollection = mongoDB.GetCollection<UserInfo>("users");

            var userInfo = await userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            IMongoCollection<BsonDocument> ShopifyOrders =
                mongoDB.GetCollection<BsonDocument>("shopify_orders");

            try
            {
                DateTime from = DateTime.Parse(fromDate);
                DateTime to = DateTime.Parse(toDate);
                var pipeline = new BsonDocument[] { };

                if (userInfo.role == "Admin" || userInfo.role == "Super Admin")
                {
                    pipeline = new[]
                     {
                        new BsonDocument("$match",
                            new BsonDocument
                            {
                                {
                                    "createdAt",
                                    new BsonDocument
                                    {
                                        { "$gte", fromDate },
                                        { "$lte", toDate }
                                    }
                                }
                            }),

                            new BsonDocument("$facet", new BsonDocument
                            {
                                { "AllOrders", new BsonArray
                                    {
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "AssignedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "has_error", false },
                                            { "accepted_by_store", "pending" },
                                            { "posted", false },
                                            { "status", new BsonDocument("$ne", "Cancelled") }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "AcceptedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "accepted_by_store", "accepted" },
                                            { "posted", false },
                                            { "has_error", false }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "DispatchedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "dispatched", true },
                                            { "posted", false },
                                            { "has_error", false }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "PostedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "dispatched", true },
                                            { "posted", true },
                                            { "has_error", false }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "CancelledOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "status", "Cancelled" }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "ErrorOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "has_error", true }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                }
                            })
                        };

                }
                else
                {
                    pipeline = new[]
                      {
                        new BsonDocument("$match",
                            new BsonDocument
                            {
                                {
                                    "createdAt",
                                    new BsonDocument
                                    {
                                        { "$gte", fromDate },
                                        { "$lte", toDate }
                                    }
                                },
                                { "assigned_store_sid", userInfo.assigned_store }
                            }),

                            new BsonDocument("$facet", new BsonDocument
                            {
                                { "AllOrders", new BsonArray
                                    {
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "AssignedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "has_error", false },
                                            { "accepted_by_store", "pending" },
                                            { "posted", false },
                                            { "status", new BsonDocument("$ne", "Cancelled") }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "AcceptedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "accepted_by_store", "accepted" },
                                            { "posted", false },
                                            { "has_error", false }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "DispatchedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "dispatched", true },
                                            { "posted", false },
                                            { "has_error", false }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "PostedOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "dispatched", true },
                                            { "posted", true },
                                            { "has_error", false }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "CancelledOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "status", "Cancelled" }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                },
                                { "ErrorOrders", new BsonArray
                                    {
                                        new BsonDocument("$match", new BsonDocument
                                        {
                                            { "has_error", true }
                                        }),
                                        new BsonDocument("$count", "count")
                                    }
                                }
                            })
                        };

                }

                var result = await ShopifyOrders.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                long GetCount(string field)
                {
                    if (result == null || !result.Contains(field))
                        return 0;

                    var array = result[field].AsBsonArray;
                    if (array.Count == 0)
                        return 0;

                    return array[0]["count"].ToInt64();
                }

                var response = new OrderCountsDto
                {
                    AllOrders = GetCount("AllOrders"),
                    AssignedOrders = GetCount("AssignedOrders"),
                    AcceptedOrders = GetCount("AcceptedOrders"),
                    DispatchedOrders = GetCount("DispatchedOrders"),
                    PostedOrders = GetCount("PostedOrders"),
                    CancelledOrders = GetCount("CancelledOrders"),
                    ErrorOrders = GetCount("ErrorOrders")
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/v1/inventory_state")]
        public async Task<IActionResult> InventoryStateInfo()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                var inventoryState = new InventoryState();

                IMongoCollection<BsonDocument> retailproInventoryLog = mongoDB.GetCollection<BsonDocument>("retailpro_inventory_log");
                IMongoCollection<BsonDocument> shopifyInventoryLog = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");

                IMongoCollection<BsonDocument> productCollection = mongoDB.GetCollection<BsonDocument>("products");
                IMongoCollection<BsonDocument> shopifyProducts = mongoDB.GetCollection<BsonDocument>("shopify_inventory");

                var retailProProductPipeline = new[]
                {
                    new BsonDocument
                    {
                        {
                            "$group",
                            new BsonDocument
                            {
                                { "_id", BsonNull.Value },
                                { "totalDocuments", new BsonDocument("$sum", 1) },
                                { "styleCount", new BsonDocument("$addToSet", "$style_sid") }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        {
                            "$project",
                            new BsonDocument
                            {
                                { "_id", 0 },
                                { "totalDocuments", 1 },
                                { "styleCount", new BsonDocument("$size", "$styleCount") }
                            }
                        }
                    }
                };

                var retailProResult = await productCollection.Aggregate<BsonDocument>(retailProProductPipeline).FirstOrDefaultAsync();

                inventoryState.RetailProItems = retailProResult?.GetValue("totalDocuments", 0).AsInt32 ?? 0;
                inventoryState.RetailProStyles = retailProResult?.GetValue("styleCount", 0).AsInt32 ?? 0;

                var shopifyPipeline = new[]
                {
                    new BsonDocument
                    {
                        {
                            "$group",
                            new BsonDocument
                            {
                                { "_id", BsonNull.Value },
                                { "totalVariantsCount", new BsonDocument("$sum", "$variantsCount.count") },
                                { "totalDocuments", new BsonDocument("$sum", 1) }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        {
                            "$project",
                            new BsonDocument
                            {
                                { "_id", 0 },
                                { "totalVariantsCount", 1 },
                                { "totalDocuments", 1 }
                            }
                        }
                    }
                };

                var shopifyResult = await shopifyProducts.Aggregate<BsonDocument>(shopifyPipeline).FirstOrDefaultAsync();
                inventoryState.ShopifyProducts = shopifyResult?.GetValue("totalDocuments", 0).AsInt32 ?? 0;
                inventoryState.ShopifyVariants = shopifyResult?.GetValue("totalVariantsCount", 0).AsInt32 ?? 0;

                inventoryState.Success = true;

                return StatusCode((int)HttpStatusCode.OK, new { inventoryState });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }

        [HttpGet]
        [Route("api/v1/order_listing")]   // , string userId
        public async Task<object> GetOrders(string orderType, string fromDate, string toDate, string Id = "NA")
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                var userInfo = await userCollection.Find(u => u.Id == Id).FirstOrDefaultAsync();
                var inventoryState = new InventoryState();

                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                IMongoCollection<BsonDocument> Shopifycancelled = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders");
                IMongoCollection<BsonDocument> ShopifyRefunds = mongoDB.GetCollection<BsonDocument>("shopify_refund_orders");

                var filter = "{}";

                if (Id == "NA" || userInfo.role == "Super Admin" || userInfo.role == "Admin")
                {
                    if (orderType == "AllOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}} }}";
                    }
                    else if (orderType == "AssignedOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, has_error: false, accepted_by_store:'pending', posted:false, status:{{$ne:""Cancelled""}} }}";
                    }
                    else if (orderType == "AcceptedOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, accepted_by_store: 'accepted', posted:false, has_error:false }}";
                    }
                    else if (orderType == "DispatchedOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, dispatched: true, posted:false, has_error:false}}";
                    }
                    else if (orderType == "PostedOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, dispatched: true, posted:true, has_error:false}}";
                    }
                    else if (orderType == "cancelledOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, status: 'Cancelled' }}";
                    }
                    else if (orderType == "errordOrders")
                    {
                        filter = @$"{{createdAt: {{ $gte:'{fromDate}'}},   has_error:true }}";
                    }

                }
                else
                {
                    

                    if (orderType == "AllOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}} }}";
                    }
                    else if (orderType == "AssignedOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, has_error: false, accepted_by_store:'pending', posted:false }}";
                    }
                    else if (orderType == "AcceptedOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, accepted_by_store: 'accepted', posted:false }}";
                    }
                    else if (orderType == "DispatchedOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, dispatched: true, posted:false}}";
                    }
                    else if (orderType == "PostedOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, dispatched: true, posted:true}}";
                    }
                    else if (orderType == "cancelledOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}, status: 'Cancelled' }}";
                    }
                    else if (orderType == "errordOrders")
                    {
                        filter = @$"{{assigned_store_sid:'{userInfo.assigned_store}', createdAt: {{ $gte:'{fromDate}'}},   has_error:true }}";
                    }
                }

                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderResultObject = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var OrderList = JsonConvert.DeserializeObject<List<DashboardOrders>>(JsonConvert.SerializeObject(orderResultObject))?.ToList();

                if (orderType == "cancelledOrders")
                {
                    filter = @$"{{createdAt: {{ $gte:'{fromDate}',  $lte:'{toDate}'}}}}";
                     var caancelledOrderResult = await Shopifycancelled.Find(filter).ToListAsync();
                    var caancelledOrderResultObject = caancelledOrderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var CancelledOrderList = JsonConvert.DeserializeObject<List<DashboardOrders>>(JsonConvert.SerializeObject(caancelledOrderResultObject))?.ToList();

                    foreach (var order in OrderList)
                    {
                        if (order.LineItemList.Count == 0)
                        {
                            var matchingCancelled = CancelledOrderList.Where(o => o.Name == order.Name).FirstOrDefault();

                            if (matchingCancelled.LineItemList.Count > 0)
                            {
                                order.LineItemList = matchingCancelled.LineItemList;
                            }
                            else
                            {
                                var items = matchingCancelled.LineItems.Edges.Select(s => s.Node).ToList();
                                order.LineItemList = items;
                            }
                        }

                    }

                }


                var startTime = DateTime.Now;

                foreach (var order in OrderList)
                {
                    TimeZoneInfo tz = TZConvert.GetTimeZoneInfo(order.BillingAddress?.TimeZone ?? order.ShippingAddress.TimeZone);

                    var refundOrderResult = await ShopifyRefunds.Find("{name:'" + order.Name + "'}").ToListAsync();

                    order.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedAt.Value.DateTime, tz);

                    
                       
                    if (order.CancelledAt == null)
                    {
                        if (refundOrderResult.Any())
                        {
                            order.type = "Return";
                        }
                        else
                        {
                            order.type = "Sale";
                        }
                    }
                    else
                    {
                        order.type = "Cancelled";
                    }
                }

                OrderInfoListing OrderListing = new OrderInfoListing { data = OrderList };

                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(OrderListing));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }

        [HttpGet]
        [Route("api/v1/inventory_listing")]
        public async Task<IActionResult> InventoryList()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            IMongoCollection<BsonDocument> productCollection = mongoDB.GetCollection<BsonDocument>("products");
            IMongoCollection<BsonDocument> ShopifyInventoryCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            try
            {

                var result = await productCollection.Find("{}").ToListAsync();
                var inventoryResultObject = result.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var InventoryListing = JsonConvert.DeserializeObject<List<InventoryListing>>(JsonConvert.SerializeObject(inventoryResultObject)).ToList();

                var shopifyQtyInfoPipeline = new[]
                {
                    // --- $unwind variantList ---
                    new BsonDocument("$unwind", "$variantList"),

                    // --- $project sku and quantities only ---
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 0 },
                        { "sku", "$variantList.sku" },
                        { "quantities", "$variantList.quantities" }
                    })
                };
                var shopifyQtyInfoPipelineResult = await ShopifyInventoryCollection.Aggregate<BsonDocument>(shopifyQtyInfoPipeline).ToListAsync();

                var shopifyQtyInfoPipelineObject = shopifyQtyInfoPipelineResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);

                var qtyJson = JsonConvert.SerializeObject(shopifyQtyInfoPipelineObject);
                var shopifyQtyInfoList = JsonConvert.DeserializeObject<List<ShopifyQtyInfo>>(qtyJson).ToList();

                foreach (var item in InventoryListing)
                {
                    var qtyInfo = shopifyQtyInfoList.Where(q => q.Sku == item.Sku).FirstOrDefault();
                    item.Available = qtyInfo?.Quantities.Where(q => q.Name == "available").FirstOrDefault()?.QuantityQuantity ?? 0;
                    item.Committed = qtyInfo?.Quantities.Where(q => q.Name == "committed").FirstOrDefault()?.QuantityQuantity ?? 0;
                    item.OnHand = qtyInfo?.Quantities.Where(q => q.Name == "on_hand").FirstOrDefault()?.QuantityQuantity ?? 0;
                }

                InventoryItemResponce inventory = new InventoryItemResponce { data = InventoryListing };

                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(inventory));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }

        [HttpGet]
        [Route("api/v1/order_detail")]
        public async Task<object> GetOrderDetail(string order_id)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                var inventoryState = new InventoryState();

                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                IMongoCollection<BsonDocument> ShopifyRefunds = mongoDB.GetCollection<BsonDocument>("shopify_refunds");
                IMongoCollection<BsonDocument> productCollection = mongoDB.GetCollection<BsonDocument>("products");

                var filter = $"{{name:'{order_id}' }}";
                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderResultObject = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var order = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderResultObject))?.FirstOrDefault();

                List<StoreQuantities> itemStoreQuantities = new List<StoreQuantities>();

                if (order.status == "Cancelled")
                {
                    var lineItems = order.LineItems?.Edges?.Select(i => i.Node).ToList();

                    if(lineItems !=null && lineItems.Any()) order.LineItemList.AddRange(lineItems);
                }

                foreach (var item in order?.LineItemList)
                {
                    var incFilter = $"{{sku:'{item.Sku}'}}";

                    var invItemResult = await productCollection.Find(incFilter).ToListAsync();
                    var invItemObject = invItemResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var inventoryItem = JsonConvert.DeserializeObject<List<InventoryListing>>(JsonConvert.SerializeObject(invItemObject))?.FirstOrDefault();

                    item.InvQuantities = inventoryItem?.StoreQuantities?.Where(s => s.STORE_NO != "3").ToList() ?? [];
                    // StoreQuantities
                }

                if (order.PaymentGatewayNames.Count > 1)
                {
                    order.PaymentGatewayNames = order.Transactions.Where(t => (!t.Status.ToUpper().Contains("FAIL") && !t.Status.ToUpper().Contains("ERR"))).Select(t => t.FormattedGateway).ToList();

                    if (order.TotalOutstandingSet.ShopMoney.Amount > 0)
                    {
                        order.PaymentGatewayNames = new List<string> { "Cash On Delivery (Cod)" };
                    }
                    else
                    {
                        order.PaymentGatewayNames.RemoveAll(r => r.ToLower().Contains("cod"));
                    }
                }

                return StatusCode((int)HttpStatusCode.OK, JsonConvert.SerializeObject(order));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/assignstore")]
        public async Task<object> AssignOrderStore([FromBody] order_assign_payload payload)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");

                var storeSidQuery = $@"select store_no , store_name from rps.store 
                                    where sid = {payload.store_no} 
                                    AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                List<JObject> storeSIdObj = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery);

                string storeName = storeSIdObj[0]["STORE_NAME"]?.ToString();
                long.TryParse(storeSIdObj[0]["STORE_NO"]?.ToString(), out long storeNo);

                var filter = $"{{name:'{payload.order_id}' }}";

                var update = Builders<BsonDocument>.Update
                    .Set("assigned_store_name", storeName)
                    .Set("assigned_store_no", storeNo)
                    .Set("assigned_store_sid", payload.store_no)
                    .Set("accepted_by_store", "pending")
                    .Set("stock_transfered", false);
                var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                var assignEvent = new EventNode
                {
                    Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                    Message = $"Order {payload.order_id} assigned to store {storeName}",
                    CreatedAt = DateTime.UtcNow,
                    __typename = "OperationalEvent",
                    Action = "Store Assigned",
                    SubjectId = "gid://shopify/Order/" + payload.order_id,
                    SubjectType = "ORDER"
                };

                var assignEventDoc = assignEvent.ToBsonDocument();
                var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                _ = await ShopifyOrders.UpdateOneAsync(filter, shopifyOrderUpdate);

                return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/update")]
        public async Task<object> UpdateOrderAddress(string order_id, [FromBody] OrderAddressUpdate payload)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");

                var updates = new List<UpdateDefinition<BsonDocument>>();
                var builder = Builders<BsonDocument>.Update;
                //orderDocument.Add(new BsonElement("courier", new BsonDocument { { "courier_name", "" }, { "cn_number", "" }, { "destination_city", order.ShippingAddress?.City }, { "destination_address", destinationAddress } }));

                if (!string.IsNullOrEmpty(payload.NewPhoneNo))
                {
                    updates.Add(builder.Set("shippingAddress.phone", payload.NewPhoneNo));
                    updates.Add(builder.Set("billingAddress.phone", payload.NewPhoneNo));
                }
                if (!string.IsNullOrEmpty(payload.NewCity))
                {
                    updates.Add(builder.Set("shippingAddress.city", payload.NewCity));
                    updates.Add(builder.Set("billingAddress.city", payload.NewCity));
                    updates.Add(builder.Set("courier.destination_city", payload.NewCity));
                }

                if (!string.IsNullOrEmpty(payload.NewCountry))
                {
                    updates.Add(builder.Set("shippingAddress.country", payload.NewCountry));
                    updates.Add(builder.Set("billingAddress.country", payload.NewCountry));
                }

                if (!string.IsNullOrEmpty(payload.NewAddress))
                {
                    updates.Add(builder.Set("shippingAddress.address1", payload.NewAddress));
                    updates.Add(builder.Set("billingAddress.address1", payload.NewAddress));
                    updates.Add(builder.Set("shippingAddress.address2", ""));
                    updates.Add(builder.Set("billingAddress.address2", ""));
                    updates.Add(builder.Set("courier.destination_address", payload.NewAddress));
                }

                if (updates.Count != 0)
                {
                    var combinedUpdate = builder.Combine(updates);

                    var filter = $"{{name:'{order_id}' }}";
                    var modified = await ShopifyOrders.UpdateOneAsync(filter, combinedUpdate);

                    return StatusCode((int)HttpStatusCode.OK, true);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.OK, new { Error = "nothing to update", Success = false });
                }

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/accept/{order_id}")]
        public async Task<object> AcceptOrder(string order_id)
        {
            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");

                var filter = $"{{name:'{order_id}' }}";

                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderResultObject = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var order = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderResultObject))?.FirstOrDefault();

                var storeSidQuery = $@"select store_no , store_name,ADDRESS1,ADDRESS2, ADDRESS3,ADDRESS4,ADDRESS5, PHONE1, UDF1_STRING as email from rps.store 
                                    where sid = {order.assigned_store_sid} 
                                    and sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})
                                    ";
                List<JObject> storeSIdObj = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery);
                var SendersAddress1 = storeSIdObj[0]["ADDRESS1"]?.ToString();
                var SendersAddress2 = storeSIdObj[0]["ADDRESS2"]?.ToString();
                var SendersCity = storeSIdObj[0]["ADDRESS4"]?.ToString();
                var SendersPhone = storeSIdObj[0]["PHONE1"]?.ToString();
                var senderEmail = storeSIdObj[0]["EMAIL"]?.ToString();

                var senderInfo = new
                {
                    SendersAddress1 = SendersAddress1,
                    SendersAddress2 = SendersAddress2,
                    SendersCity = SendersCity,
                    SendersPhone = SendersPhone,
                    SenderEmail = senderEmail
                };

                var update = Builders<BsonDocument>.Update
                    .Set("accepted_by_store", "accepted")
                    .Set("is_courier_assigned", false)
                    .Set("sender_info", senderInfo);
                var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                string graphQLOrderId = $"gid://shopify/Order/{order.OrderId}";

                if (result.ModifiedCount > 0)
                {
                    var request = new GraphQLRequest
                    {
                        Query = @"
                        mutation SetOrderComment($id: ID!, $note: String!) {
                          orderUpdate(input: {id: $id, note: $note}) {
                            order {
                              id
                              note
                              name
                            }
                            userErrors {
                              field
                              message
                            }
                          }
                        }",
                        Variables = new
                        {
                            id = graphQLOrderId,
                            note = $"Order Accepted by store: {order.assigned_store_name}"
                        }
                    };
                    _ = await GraphAPI.SendMutation(request);
                }

                if (result.ModifiedCount > 0 && GlobalVariables.RProConfig.TestMode)
                {
                    Random random = new Random();

                    var TestCourierUpdate = Builders<BsonDocument>.Update
                        .Set("accepted_by_store", "accepted")
                        .Set("courier.courier_name", "TEST")
                        .Set("courier.cn_number", "OMNI" + random.Next(100000, 1000000))
                        .Set("is_courier_assigned", true);

                    _ = await ShopifyOrders.UpdateOneAsync(filter, TestCourierUpdate);
                }



                var assignEvent = new EventNode
                {
                    Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                    Message = $"Order {order.Name} with Id {order.OrderId} is accepted by store {order.assigned_store_name}",
                    CreatedAt = DateTime.UtcNow,
                    __typename = "OperationalEvent",
                    Action = "Accepted by Store",
                    SubjectId = "gid://shopify/Order/" + order.OrderId,
                    SubjectType = "ORDER"
                };

                var assignEventDoc = assignEvent.ToBsonDocument();
                var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                _ = await ShopifyOrders.UpdateOneAsync(filter, shopifyOrderUpdate);

                return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/reject/{order_id}")]
        public async Task<object> RejectOrder(string order_id)
        {
            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                var filter = $"{{name:'{order_id}' }}";

                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderResultObject = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var order = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderResultObject))?.FirstOrDefault();



                var update = Builders<BsonDocument>.Update
                    .Set("dispatched", false)
                    .Set("courier.courier_name", BsonNull.Value)
                    .Set("courier.cn_number", BsonNull.Value)
                    //.Set("assigned_store_name", BsonNull.Value)
                    .Set("assigned_store_sid", BsonNull.Value)
                    .Set("assigned_store_no", BsonNull.Value)
                    .Set("accepted_by_store", "rejected");

                var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                var assignEvent = new EventNode
                {
                    Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                    Message = $"Order {order.Name} with Id {order.OrderId} is rejected by store {order.assigned_store_name}",
                    CreatedAt = DateTime.UtcNow,
                    __typename = "OperationalEvent",
                    Action = "Rejected By Store",
                    SubjectId = "gid://shopify/Order/" + order.OrderId,
                    SubjectType = "ORDER"
                };

                var assignEventDoc = assignEvent.ToBsonDocument();
                var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                _ = await ShopifyOrders.UpdateOneAsync(filter, shopifyOrderUpdate);

                return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/dispatch")]
        public async Task<object> DispatchOrderFromStore([FromBody] order_assign_payload payload)
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                var filter = $"{{name:'{payload.order_id}' }}";

                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderResultObject = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var order = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderResultObject))?.FirstOrDefault();



                var update = Builders<BsonDocument>.Update
                    .Set("dispatched", true);
                var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                var assignEvent = new EventNode
                {
                    Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                    Message = $"Order {order.Name} with Id {order.OrderId} is dispatched by store {order.assigned_store_name}",
                    CreatedAt = DateTime.UtcNow,
                    __typename = "OperationalEvent",
                    Action = "Order Dispatched",
                    SubjectId = "gid://shopify/Order/" + order.OrderId,
                    SubjectType = "ORDER"
                };

                var assignEventDoc = assignEvent.ToBsonDocument();
                var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                _ = await ShopifyOrders.UpdateOneAsync(filter, shopifyOrderUpdate);


                return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/release")]
        public async Task<object> CancelRelease([FromBody] order_assign_payload payload)
        {
            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                IMongoCollection<BsonDocument> ShopifyOrdersCancelled = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders");
                var filter = $"{{name:'{payload.order_id}' }}";

                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var order = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderObj))?.FirstOrDefault() ?? new OrderForListing();

                if (order.stock_transfered)
                {
                    // return stock

                    PluginManager.store_Sbs_SID OrderStore_info = new PluginManager.store_Sbs_SID();

                    var storeSIdQuery = $"select STORE_NO, to_char(sid) as STORE_SID, to_char(sbs_sid) as SBS_SID from RPS.STORE " +
                    $" where store_no = {GlobalVariables.RProConfig.OrderStoreNo}  and sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                    OrderStore_info = ADO.ReadAsync<PluginManager.store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new(); //connection.Query<store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new();

                    #region Stock return to Source Store

                    var AssignedStoreSid = order.assigned_store_sid; // existingInvoice["assigned_store_sid"].ToString();

                    var storeSidQuery = $@"select to_char(sbs_sid) as sbs_sid, store_name from rps.store 
                                           where 1=1 AND sid = {AssignedStoreSid} 
                                           AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";

                    JObject inStoreInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery)?.FirstOrDefault() ?? new();
                    var instoreSbsSid = inStoreInfo["SBS_SID"]?.ToString() ?? "NA";
                    var inStoreSid = AssignedStoreSid;

                    var items = order.LineItemList.ToList();//.Fulfillments.SelectMany(f => f.FulfillmentLineItemList).ToList();

                    var slipItemsList = new List<SlipItemData>();
                    foreach (var item in items)
                    {
                        var itemSidQuery = $@"
                                            select to_char(sid) as SID from rps.invn_sbs_item                                         
                                            where ALU = '{item.Sku}' 
                                            AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})
                                            ";
                        JObject itemSidInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(itemSidQuery)?.FirstOrDefault() ?? new();
                        var itemSid = itemSidInfo["SID"].ToString();

                        slipItemsList.Add(new SlipItemData
                        {
                            originapplication = "RProPrismWeb",
                            itemsid = itemSid,
                            slipsid = "NA",
                            qty = item.Quantity
                        });
                    }

                    var transferSlipUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip";
                    var slipPayload = $@"
                                                        {{
                                                            ""data"": [
                                                                {{
                                                                    ""originapplication"": ""RProPrismWeb"",
                                                                    ""status"": 3,
                                                                    ""docreasonsid"": null,
                                                                    ""insbssid"": ""{instoreSbsSid}"",
                                                                    ""instoresid"": ""{inStoreSid}"",
                                                                    ""outsbssid"": ""{OrderStore_info.SBS_SID}"",
                                                                    ""outstoresid"":""{OrderStore_info.STORE_SID}""
                                                                }}
                                                            ]
                                                        }}
                                                        ";
                    // Slip Posting

                    var slipResponse = await RetailPro2_X.APICall.Post(transferSlipUrl, slipPayload, GlobalVariables.RetailProAuthSession);
                    var responseData = JsonConvert.DeserializeObject<AsnResponse>(slipResponse.Content ?? "{}");
                    var SlipSid = responseData.Data.Select(d => d.Sid).FirstOrDefault();

                    // Slip Item Posting
                    var transferSlitItemUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}/slipitem";
                    slipItemsList.ForEach(i => i.slipsid = SlipSid);

                    RootObject rootObject = new RootObject { data = slipItemsList };
                    var slipItemResponse = await RetailPro2_X.APICall.Post(transferSlitItemUrl, JsonConvert.SerializeObject(rootObject), GlobalVariables.RetailProAuthSession);

                    var commentsUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/slipcomment?comments=Shopify Order No : {order.Name}&slipsid={SlipSid}";
                    var commentsPayload = $@"{{""data"":[{{""originapplication"":""RProPrismWeb"",""slipsid"":""{SlipSid}"",""comments"":""Shopify Order No : {order.Name}""}}]}}";
                    var commentsPostResponse = await RetailPro2_X.APICall.Post(commentsUrl, commentsPayload, GlobalVariables.RetailProAuthSession);

                    // get RowVersion to finalize slip
                    var slipRowversionURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}";
                    var slipRowversionResponse = await RetailPro2_X.APICall.GetAsync(slipRowversionURL, GlobalVariables.RetailProAuthSession);
                    var slipRowversionData = JsonConvert.DeserializeObject<JObject>(slipRowversionResponse.Content ?? "{}");
                    int.TryParse(slipRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int rowversion);

                    // finalize slip
                    var slipFinalizeUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}";
                    var slipFinalizePayload = $@"
                                                        {{
                                                            ""data"": [
                                                                {{
                                                                    ""originapplication"": ""RProPrismWeb"",
                                                                    ""rowversion"": {rowversion},
                                                                    ""status"": 4
                                                                }}
                                                            ]
                                                        }}
                                                        ";
                    var slipFinalized = await RetailPro2_X.APICall.PUT(slipFinalizeUrl, slipFinalizePayload, GlobalVariables.RetailProAuthSession);

                    var slipfinalizedData = JsonConvert.DeserializeObject<JObject>(slipFinalized.Content ?? "{}");
                    var voucherSid = slipfinalizedData?["data"]?[0]?["vousid"]?.ToString();
                    // vousid
                    if (slipFinalized.IsSuccessful)
                    {
                        var convertAsnToVoucherURL = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving?action=convertasntovoucher";
                        var convertVoucherPayload = $@"
                                                            {{
                                                                ""data"": [
                                                                    {{
                                                                        ""clerksid"": ""745052955000115352"",
                                                                        ""asnsidlist"": ""{voucherSid}"",
                                                                        ""doupdatevoucher"": false,
                                                                        ""originapplication"": ""RProPrismWeb""
                                                                    }}
                                                                ]
                                                            }}
                                                            ";
                        var convertResponse = await RetailPro2_X.APICall.Post(convertAsnToVoucherURL, convertVoucherPayload, GlobalVariables.RetailProAuthSession);
                        var voucherURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving/{voucherSid}";
                        var voucherRowversionResponse = await RetailPro2_X.APICall.GetAsync(voucherURL, GlobalVariables.RetailProAuthSession);
                        var voucherRowversionData = JsonConvert.DeserializeObject<JObject>(voucherRowversionResponse.Content ?? "{}");
                        int.TryParse(voucherRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int vRowversion);

                        var voucherFinalizePayload = $@"
                                                            {{
                                                                ""data"": [
                                                                    {{
                                                                        ""rowversion"": {vRowversion},
                                                                        ""status"": 4,
                                                                        ""approvstatus"": 2,
                                                                        ""publishstatus"": 2
                                                                    }}
                                                                ]
                                                            }}
                                                            ";

                        var voucherFinalizeResponse = await RetailPro2_X.APICall.PUT(voucherURL, voucherFinalizePayload, GlobalVariables.RetailProAuthSession);

                        if (voucherFinalizeResponse.IsSuccessful)
                        {
                            var update = Builders<BsonDocument>.Update
                            .Set("accepted_by_store", "")
                            .Set("order_error_state", "")
                            .Set("assigned_store_no", -1)
                            .Set("assigned_store_sid", 0)
                            .Set("courier.courier_name", "")
                            .Set("courier.cn_number", "")
                            .Set("is_courier_assigned", false)
                            .Set("stock_transfered", false)
                            .Set("assigned_store_name", "")
                            .Unset("sender_info")
                            .Unset("status");

                            var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                            var assignEvent = new EventNode
                            {
                                Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                                Message = $"Order {order.Name} with Id {order.OrderId} is released by store {order.assigned_store_name}",
                                CreatedAt = DateTime.UtcNow,
                                __typename = "OperationalEvent",
                                Action = "Order Released",
                                SubjectId = "gid://shopify/Order/" + order.OrderId,
                                SubjectType = "ORDER"
                            };

                            var assignEventDoc = assignEvent.ToBsonDocument();
                            var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                            _ = await ShopifyOrders.UpdateOneAsync(filter, shopifyOrderUpdate);

                            return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
                        }
                        else
                        {
                            BsonDocument document1 = new BsonDocument();
                            document1["created_at"] = DateTime.Now;
                            document1["service"] = "Shopify Refunds";
                            document1["exception_message"] = "Voucher Not Generated";
                            document1["exception_source"] = "Prism API";
                            document1["exception_stack_trace"] = voucherFinalizeResponse.Content;

                            await exceptionCollection.InsertOneAsync(document1);
                        }
                    }
                    else
                    {
                        BsonDocument document1 = new BsonDocument();
                        document1["created_at"] = DateTime.Now;
                        document1["service"] = "Shopify Refunds";
                        document1["exception_message"] = "ASN Not Generated";
                        document1["exception_source"] = "Prism API";
                        document1["exception_stack_trace"] = slipFinalized.Content;

                        await exceptionCollection.InsertOneAsync(document1);
                    }

                    #endregion Stock return to Source Store
                }
                else
                {
                    // rderDocument.Add(new BsonElement("courier", new BsonDocument { { "courier_name", "" }, { "cn_number", "" }, { "destination_city", order.ShippingAddress?.City }, { "destination_address", destinationAddress } }));
                    var update = Builders<BsonDocument>.Update
                    .Set("accepted_by_store", "")
                    .Set("order_error_state", "")
                    .Set("assigned_store_no", -1)
                    .Set("assigned_store_sid", 0)
                    .Set("courier.courier_name", "")
                    .Set("courier.cn_number", "")
                    .Set("is_courier_assigned", false)
                    .Set("stock_transfered", false)
                    .Set("assigned_store_name", "")
                    .Unset("sender_info")
                    .Unset("status");

                    var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                    var assignEvent = new EventNode
                    {
                        Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                        Message = $"Order {order.Name} with Id {order.OrderId} is released by store {order.assigned_store_name}",
                        CreatedAt = DateTime.UtcNow,
                        __typename = "OperationalEvent",
                        Action = "Order Released",
                        SubjectId = "gid://shopify/Order/" + order.OrderId,
                        SubjectType = "ORDER"
                    };

                    var assignEventDoc = assignEvent.ToBsonDocument();
                    var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                    _ = await ShopifyOrders.UpdateOneAsync(filter, shopifyOrderUpdate);

                    return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = "", Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpPost]
        [Route("api/v1/order/cancel/{order_id}")]
        public async Task<object> CancelOrder(string order_id, [FromBody] OrderAddressUpdate payload)
        {
            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                IMongoCollection<BsonDocument> ShopifyOrdersCancelled = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders");
                var filter = $"{{name:'{order_id}' }}";

                var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var order = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderObj))?.FirstOrDefault() ?? new OrderForListing();

                if (order.PaymentGatewayNames.Count == 1 && order.PaymentGatewayNames.First() == "Cash on Delivery (COD)")
                { // cancle Order in OMNI and Shopify
                }

                var update = Builders<BsonDocument>.Update
                   .Set("cancelledAt", DateTime.Now)
                   .Set("cancelReason", payload.CancelReason)
                   .Set("status", "cancelled");

                var result = await ShopifyOrders.UpdateOneAsync(filter, update);

                //_ = ShopifyOrdersCancelled.InsertOneAsync(order);

                return StatusCode((int)HttpStatusCode.OK, result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { Error = ex.Message, Success = false });
            }
        }

        [HttpGet]
        [Route("api/v1/store/list")]
        public async Task<object> GetStoreList()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            try
            {
                var storeSidQuery = $@"select to_char(sid) as sid, store_name from rps.store 
                                    where 1=1 
                                    AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO}) 
                                    AND active = 1 
                                    and store_no not in (0,3)";
                
                List<PluginManager.StoreInfo> stroreList = RetailPro2_X.BL.ADO.ReadAsync<PluginManager.StoreInfo>(storeSidQuery);

                return StatusCode((int)HttpStatusCode.OK, stroreList);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new ServiceStatusInfo { Error = ex.Message, Success = false });
            }
        }

        /*

             [HttpGet]
             [Route("api/v1/inventory_difference")]
             public async Task<HttpResponseMessage> InventoryDifference(string fromDate, string toDate)
             {
                 MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
                 var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);

                 try
                 {
                     var inventoryState = new InventoryState();

                     IMongoCollection<BsonDocument> exceptionLog = mongoDB.GetCollection<BsonDocument>("exception_log");

                     var filter = "{service:'Shopify Map Inventory'}";
                     var inventoryExceptions = await exceptionLog.Find(filter).ToListAsync();
                     var inventoryExceptionObject = inventoryExceptions.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                     var inventoryExceptionList = JsonConvert.DeserializeObject<List<ShopifyInvExceptions>>(JsonConvert.SerializeObject(inventoryExceptionObject)).FirstOrDefault();

                     return Request.CreateResponse(HttpStatusCode.OK, inventoryState, Configuration.Formatters.JsonFormatter);
                 }
                 catch (Exception ex)
                 {
                     return Request.CreateResponse(HttpStatusCode.OK, new InventoryState { Error = ex.Message, Success = false }, Configuration.Formatters.JsonFormatter);
                 }
             }

             [HttpGet]
             [Route("api/v1/order_exception_log")]
             public async Task<HttpResponseMessage> OrderExceptionLog(string fromDate, string toDate)
             {
                 MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
                 var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);

                 try
                 {
                     var inventoryState = new InventoryState();

                     IMongoCollection<BsonDocument> exceptionLog = mongoDB.GetCollection<BsonDocument>("exception_log");
                     var orderServices = $"'RetailPro Refund Build Service','RetailPro Order Build Service','Fetch Shopify Orders','RetailPro Invoice Matching'";
                     var filter = "{service:{$in:[" + orderServices + "]},created_at: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";
                     var exceptionLogResult = await exceptionLog.Find(filter).ToListAsync();
                     var exceptionLogObject = exceptionLogResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                     var exceptionLogList = JsonConvert.DeserializeObject<List<ExceptionLog>>(JsonConvert.SerializeObject(exceptionLogObject)).ToList();
                     ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo { data = exceptionLogList };

                     return Request.CreateResponse(HttpStatusCode.OK, exceptionLogInfo, Configuration.Formatters.JsonFormatter);
                 }
                 catch (Exception ex)
                 {
                     return Request.CreateResponse(HttpStatusCode.OK, new InventoryState { Error = ex.Message, Success = false }, Configuration.Formatters.JsonFormatter);
                 }
             }

             [HttpGet]
             [Route("api/v1/order_listing")]
             public async Task<HttpResponseMessage> OrderInfoListing(string fromDate, string toDate)
             {
                 MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
                 var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);

                 try
                 {
                     var inventoryState = new InventoryState();

                     IMongoCollection<BsonDocument> ShopifyOrders = mongoDB.GetCollection<BsonDocument>("shopify_orders");
                     IMongoCollection<BsonDocument> ShopifyRefunds = mongoDB.GetCollection<BsonDocument>("shopify_refunds");

                     var filter = "{createdAt: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";
                     var orderResult = await ShopifyOrders.Find(filter).ToListAsync();
                     var orderResultObject = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                     var OrderList = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderResultObject)).ToList();

                     foreach (var order in OrderList)
                     {
                         var refundOrderResult = await ShopifyRefunds.Find("{name:'" + order.Name + "'}").ToListAsync();

                         if (refundOrderResult.Any())
                         {
                             order.status = "Return";
                         }
                         else
                         {
                             order.status = "Sale";
                         }
                     }

                     OrderInfoListing OrderListing = new OrderInfoListing { data = OrderList };
                     return Request.CreateResponse(HttpStatusCode.OK, OrderListing, Configuration.Formatters.JsonFormatter);
                 }
                 catch (Exception ex)
                 {
                     return Request.CreateResponse(HttpStatusCode.OK, new OrderInfoListing { data = new List<OrderForListing>() }, Configuration.Formatters.JsonFormatter);
                 }
             }

             [HttpGet]
             [Route("api/v1/inventory_exception_log")]
             public async Task<HttpResponseMessage> InventoryExceptionLog(string fromDate, string toDate)
             {
                 MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
                 var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);

                 try
                 {
                     var inventoryState = new InventoryState();

                     IMongoCollection<BsonDocument> exceptionLog = mongoDB.GetCollection<BsonDocument>("exception_log");
                     var invServices = $"'Fetch Shopify Inventory','Shopify Map Inventory','Shopify Item Create','Shopify Update Price','Shopify Update Quantity','RetailPro Inventory Pull Service'";
                     var filter = "{service:{$in:[" + invServices + "]},created_at: { $gte:('" + fromDate + "') ,  $lte:('" + toDate + "') } }";
                     var exceptionLogResult = await exceptionLog.Find(filter).ToListAsync();
                     var exceptionLogObject = exceptionLogResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                     var exceptionLogList = JsonConvert.DeserializeObject<List<ExceptionLog>>(JsonConvert.SerializeObject(exceptionLogObject)).ToList();
                     ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo { data = exceptionLogList };

                     return Request.CreateResponse(HttpStatusCode.OK, exceptionLogInfo, Configuration.Formatters.JsonFormatter);
                 }
                 catch (Exception ex)
                 {
                     return Request.CreateResponse(HttpStatusCode.OK, new InventoryState { Error = ex.Message, Success = false }, Configuration.Formatters.JsonFormatter);
                 }
             }

             [HttpGet]
             [Route("api/v1/agent_health")]
             public async Task<HttpResponseMessage> GetAgentHealth()
             {
                 try
                 {
                     MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
                     var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
                     IMongoCollection<BsonDocument> Agenthealth = mongoDB.GetCollection<BsonDocument>("agent_health");

                     var filter = "{}";
                     var foundResult = await Agenthealth.Find(filter).ToListAsync();
                     var inventoryResultObject = foundResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                     var found = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(inventoryResultObject)).FirstOrDefault();

                     DateTime.TryParse(found["update_at"].ToString(), out DateTime foundDate);

                     return Request.CreateResponse(HttpStatusCode.OK, new { found = foundDate.ToString("MM/dd/yyyy HH:mm:ss") }, Configuration.Formatters.JsonFormatter);
                 }
                 catch (Exception ex)
                 {
                     return Request.CreateResponse(HttpStatusCode.OK, false, Configuration.Formatters.JsonFormatter);
                 }
             }
             */
    }

    public partial class ShopifyInvExceptions
    {
        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("exception_message")]
        public string ExceptionMessage { get; set; }

        [JsonProperty("exception_source")]
        public string ExceptionSource { get; set; }

        [JsonProperty("exception_stack_trace")]
        public string ExceptionStackTrace { get; set; }

        [JsonProperty("Shopify Product Id")]
        public string ShopifyProductId { get; set; }

        [JsonProperty("Shopify Variant Id")]
        public string ShopifyVariantId { get; set; }
    }

    public class ServiceStatus
    {
        public string service { get; set; }
        public bool enabled { get; set; }
    }

    public class ServiceStatusInfo
    {
        public bool InventoryService { get; set; }
        public bool OrderService { get; set; }
        public DateTimeOffset? InvnSrvLastRunAt { get; set; }
        public string InvTimeElapsed { get; set; }
        public DateTimeOffset? OrderSrvLastRunAt { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; }
    }

    public class InventoryState
    {
        public long RetailProStyles { get; set; }
        public long RetailProItems { get; set; }
        public long ShopifyProducts { get; set; }
        public long ShopifyVariants { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; }
    }

    public class OrderState
    {
        public long ShopifyWebOrders { get; set; }
        public long ShopifyDownloadedOrders { get; set; }
        public long RetailProInvoicesProcessed { get; set; }
        public long RetailProInvoicesFound { get; set; }
        public long OrdersInErrorState { get; set; }
    }

    public class ExceptionLogInfo
    {
        [JsonProperty("data")]
        public List<ExceptionLog> data { get; set; }
    }

    public partial class ExceptionLog
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("order_number")]
        public string order_number { get; set; }

        [JsonProperty("sku_alu")]
        public string sku_alu { get; set; }

        [JsonProperty("Shopify Product Id")]
        public string ShopifyProductId { get; set; }

        [JsonProperty("Shopify Variant Id")]
        public string ShopifyVariantId { get; set; }

        [JsonProperty("exception_message")]
        public string ExceptionMessage { get; set; }

        [JsonProperty("exception_source")]
        public string ExceptionSource { get; set; }

        [JsonProperty("exception_stack_trace")]
        public string ExceptionStackTrace { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset createdAt { get; set; }
    }

    public class OrderInfoListing
    {
        [JsonProperty("data")]
        public List<DashboardOrders> data { get; set; }
    }

    public class OrderForListing : OrderNode
    {
        //public bool xml_generated { get; set; } = false;
        //public bool refund_xml_generated { get; set; } = false;
        [JsonProperty("retailProSid")]
        public string RetailProSid { get; set; }

        public bool has_error { get; set; } = false;
        public string status { get; set; } = "";
        public string type { get; set; } = "";
        public string assigned_store_no { get; set; } = "";
        public string assigned_store_sid { get; set; } = "";
        public string assigned_store_name { get; set; } = "";
        public bool dispatched { get; set; } = false;
        public bool stock_transfered { get; set; } = false;
        public bool is_courier_assigned { get; set; } = false;

        [JsonProperty("courier")]
        public Courier Courier { get; set; }

        public string accepted_by_store { get; set; } = "pending";
        public string error_message { get; set; } = "";

        public bool isCancelled { get; set; } = false;
        public bool posted { get; set; } = false;
    }


    public class DashboardOrders
    {
        //public bool xml_generated { get; set; } = false;
        //public bool refund_xml_generated { get; set; } = false;
        [JsonProperty("retailProSid")]
        public string RetailProSid { get; set; }

        public bool has_error { get; set; } = false;
        public string status { get; set; } = "";
        public string type { get; set; } = "";
        public string assigned_store_no { get; set; } = "";
        public string assigned_store_sid { get; set; } = "";
        public string assigned_store_name { get; set; } = "";
        public bool dispatched { get; set; } = false;
        public bool stock_transfered { get; set; } = false;
        public bool is_courier_assigned { get; set; } = false;

        [JsonProperty("courier")]
        public Courier Courier { get; set; }

        public string accepted_by_store { get; set; } = "pending";
        public string error_message { get; set; } = "";

        public bool isCancelled { get; set; } = false;
        public bool posted { get; set; } = false;

        //[BsonId] // Marks this as the MongoDB document ID
        //[BsonRepresentation(BsonType.ObjectId)] // Converts it from string to ObjectId automatically
        //public string? _Id { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        //[JsonProperty("order_id")]
        //public long OrderId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("email")]
        //public string Email { get; set; }

        //[JsonProperty("phone")]
        //public string Phone { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("closedAt")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonProperty("cancelledAt")]
        public DateTimeOffset? CancelledAt { get; set; }

        [JsonProperty("cancelReason")]
        public string? CancelReason { get; set; }

        [JsonProperty("processedAt")]
        public DateTimeOffset? ProcessedAt { get; set; }

        //[JsonProperty("fullyPaid")]
        //public bool FullyPaid { get; set; }

        //[JsonProperty("unpaid")]
        //public bool Unpaid { get; set; }

        //[JsonProperty("currencyCode")]
        //public string CurrencyCode { get; set; }

        //[JsonProperty("displayFinancialStatus")]
        //public string DisplayFinancialStatus { get; set; }

        //[JsonProperty("displayFulfillmentStatus")]
        //public string DisplayFulfillmentStatus { get; set; }

        //[JsonProperty("edited")]
        //public bool Edited { get; set; }

        //[JsonProperty("tags")]
        //public List<String>? Tags { get; set; }

        //[JsonProperty("events")]
        //public List<EventNode> Events { get; set; } = [];

        [JsonProperty("shippingAddress")]
        public PluginManager.Address ShippingAddress { get; set; }

        [JsonProperty("billingAddress")]
        public PluginManager.Address BillingAddress { get; set; }

        //[JsonProperty("customer")]
        //public Customer Customer { get; set; }

        [JsonProperty("lineItems")]
        public LineItems? LineItems { get; set; }

        [JsonProperty("lineItemList")]
        public List<LineItemNode> LineItemList { get; set; } = [];

        //[JsonProperty("fulfillmentsCount")]
        //public FulfillmentsCount FulfillmentsCount { get; set; }

        //[JsonProperty("fulfillments")]
        //public List<Fulfillment> Fulfillments { get; set; }

        //[JsonProperty("netPaymentSet")]
        //public MoneyBag NetPaymentSet { get; set; }

        //[JsonProperty("note")]
        //public string Note { get; set; }

        //[JsonProperty("originalTotalAdditionalFeesSet")]
        //public object OriginalTotalAdditionalFeesSet { get; set; }

        //[JsonProperty("originalTotalDutiesSet")]
        //public object OriginalTotalDutiesSet { get; set; }

        //[JsonProperty("originalTotalPriceSet")]
        //public MoneyBag OriginalTotalPriceSet { get; set; }

        [JsonProperty("currentTotalPriceSet")]
        public MoneyBag CurrentTotalPriceSet { get; set; }

        //[JsonProperty("currentSubtotalPriceSet")]
        //public MoneyBag CurrentSubtotalPriceSet { get; set; }

        [JsonProperty("paymentGatewayNames")]
        public List<string> PaymentGatewayNames { get; set; }

        //[JsonProperty("presentmentCurrencyCode")]
        //public string PresentmentCurrencyCode { get; set; }

        //[JsonProperty("refundable")]
        //public bool Refundable { get; set; }

        //[JsonProperty("requiresShipping")]
        //public bool RequiresShipping { get; set; }

        //[JsonProperty("returnStatus")]
        //public string ReturnStatus { get; set; }

        //[JsonProperty("taxesIncluded")]
        //public bool TaxesIncluded { get; set; }

        //[JsonProperty("taxExempt")]
        //public bool TaxExempt { get; set; }

        //[JsonProperty("taxLines")]
        //public List<TaxLine> TaxLines { get; set; }

        //[JsonProperty("totalDiscountsSet")]
        //public MoneyBag TotalDiscountsSet { get; set; }

        //[JsonProperty("totalOutstandingSet")]
        //public MoneyBag TotalOutstandingSet { get; set; }

        //[JsonProperty("totalPriceSet")]
        //public MoneyBag TotalPriceSet { get; set; }

        //[JsonProperty("totalReceivedSet")]
        //public MoneyBag TotalReceivedSet { get; set; }

        //[JsonProperty("totalRefundedSet")]
        //public MoneyBag TotalRefundedSet { get; set; }

        //[JsonProperty("totalRefundedShippingSet")]
        //public MoneyBag TotalRefundedShippingSet { get; set; }

        //[JsonProperty("totalShippingPriceSet")]
        //public MoneyBag TotalShippingPriceSet { get; set; }

        //[JsonProperty("totalTaxSet")]
        //public MoneyBag TotalTaxSet { get; set; }

        //[JsonProperty("refunds")]
        //public List<Refund> Refunds { get; set; }

        //[JsonProperty("transactions")]
        //public List<Transaction> Transactions { get; set; }

        //[JsonProperty("isCancelled")]
        //public bool IsCancelled { get; set; } = false;

    }

    public partial class Courier
    {
        [JsonProperty("courier_name")]
        public string CourierName { get; set; }

        [JsonProperty("cn_number")]
        public string CnNumber { get; set; }

        [JsonProperty("destination_city")]
        public string DestinationCity { get; set; }

        [JsonProperty("destination_address")]
        public string DestinationAddress { get; set; }
    }

    public class InventoryItem
    {
        [JsonProperty("sid")]
        public string SID { get; set; }

        [JsonProperty("sbs_no")]
        public int SBS_NO { get; set; }

        [JsonProperty("created_datetime")]
        public string CREATED_DATETIME { get; set; }

        [JsonProperty("style_sid")]
        public string STYLE_SID { get; set; }

        [JsonProperty("alu")]
        public string ALU { get; set; }

        [JsonProperty("sku")]
        public string SKU { get; set; }

        [JsonProperty("upc")]
        public string UPC { get; set; }

        [JsonProperty("description1")]
        public string DESCRIPTION1 { get; set; }

        [JsonProperty("description2")]
        public string DESCRIPTION2 { get; set; }

        [JsonProperty("description3")]
        public string DESCRIPTION3 { get; set; }

        [JsonProperty("description4")]
        public string DESCRIPTION4 { get; set; }

        [JsonProperty("attribute")]
        public string ATTRIBUTE { get; set; }

        [JsonProperty("item_size")]
        public string ITEM_SIZE { get; set; }

        [JsonProperty("long_description")]
        public string LONG_DESCRIPTION { get; set; }

        [JsonProperty("kit_type")]
        public int KIT_TYPE { get; set; }

        [JsonProperty("cost")]
        public decimal? COST { get; set; }

        [JsonProperty("store_oh")]
        public long? STORE_OH { get; set; }

        [JsonProperty("combined_oh")]
        public long? COMBINED_OH { get; set; }

        [JsonProperty("prices")]
        public object PRICES { get; set; }

        [JsonProperty("has_qty")]
        public bool HasQty { get; set; }
    }

    public class InventoryItemResponce
    {
        [JsonProperty("data")]
        public List<InventoryListing> data { get; set; }
    }

    public class ConfigurationVM
    {
        public string client_name { get; set; } = "";

        //public string platform1 { get; set; } = "RetailPro";
        public string platform { get; set; } = "";

        public string platform1 { get; set; } = "RetailPro";
        public string platform2 { get; set; } = "Shopify";
        public string platform_version { get; set; } = "";
        public string db_user_name { get; set; } = "reportuser";
        public string db_password { get; set; } = "report";
        public string server_address { get; set; } = "";
        public string db_server_port { get; set; } = "1521";
        public string db_sid { get; set; } = "RPROODS";

        //public string platform2 { get; set; } = "Shopify";
        public string store_identifier { get; set; } = "";

        public string shopify_location_name { get; set; }
        public string api_key { get; set; } = "";
        public string api_access_token { get; set; } = "";
        public bool inventory_service { get; set; }
        public bool order_service { get; set; }

        public RetailProConfigurationInfo retailpro { get; set; }
        public ShopifyConfigurationInfo shopify { get; set; }
    }

    public class RetailProConfigurationInfo
    {
        public string client_name { get; set; } = "";

        [JsonProperty("platform1")]
        public string platform { get; set; } = "RetailPro";

        public string platform_version { get; set; } = "";
        public string db_user_name { get; set; } = "reportuser";
        public string db_password { get; set; } = "report";
        public string server_address { get; set; } = "";
        public string db_server_port { get; set; } = "1521";
        public string db_sid { get; set; } = "RPROODS";
        public string prism_user { get; set; } = "RPROODS";
        public string prism_password { get; set; } = "RPROODS";
    }

    public class ShopifyConfigurationInfo
    {
        public string client_name { get; set; } = "";
        [JsonProperty("platform2")] public string platform { get; set; } = "Shopify";
        public string store_identifier { get; set; } = "";
        public string shopify_location_name { get; set; }
        public string api_key { get; set; } = "";
        public string api_access_token { get; set; } = "";
    }

    //public partial class ServiceConfigurationInfo
    //{
    //    public string client_name { get; set; } = "";

    //    public bool inventory_service { get; set; }
    //    public bool order_service { get; set; }

    //    [JsonProperty("service")]
    //    public string service { get; set; }

    //    [JsonProperty("enabled")]
    //    public bool enabled { get; set; }

    //}

    public partial class InventoryListing
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("sbs_no")]
        public int SBS_NO { get; set; }

        [JsonProperty("created_datetime")]
        public string? CreatedDatetime { get; set; }

        [JsonProperty("style_sid")]
        public string? StyleSid { get; set; }

        [JsonProperty("alu")]
        public string? Alu { get; set; }

        [JsonProperty("sku")]
        public string? Sku { get; set; }

        [JsonProperty("upc")]
        public string? Upc { get; set; }

        [JsonProperty("description1")]
        public string Description1 { get; set; }

        [JsonProperty("description2")]
        public string? Description2 { get; set; }

        [JsonProperty("description3")]
        public string? Description3 { get; set; }

        [JsonProperty("description4")]
        public string? Description4 { get; set; }

        [JsonProperty("attribute")]
        public string? Attribute { get; set; }

        [JsonProperty("item_size")]
        public string? ItemSize { get; set; }

        [JsonProperty("long_description")]
        public string? LongDescription { get; set; }

        [JsonProperty("kit_type")]
        public long? KitType { get; set; }

        [JsonProperty("cost")]
        public double? Cost { get; set; }

        [JsonProperty("store_oh")]
        public long? StoreOh { get; set; }

        [JsonProperty("combined_oh")]
        public long? CombinedOh { get; set; }

        [JsonProperty("prices")]
        public Prices? Prices { get; set; }

        [JsonProperty("has_qty")]
        public bool? HasQty { get; set; }

        [JsonProperty("productId")]
        public string? ProductId { get; set; }

        [JsonProperty("variantId")]
        public string? VariantId { get; set; }

        [JsonProperty("store_quantities")]
        public List<StoreQuantities>? StoreQuantities { get; set; } = [];

        [JsonProperty("available")]
        public long? Available { get; set; }

        [JsonProperty("committed")]
        public long? Committed { get; set; }

        [JsonProperty("onHand")]
        public long OnHand { get; set; }
    }

    public partial class ShopifyQtyInfo
    {
        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("quantities")]
        public List<Quantity> Quantities { get; set; }
    }

    public partial class Quantity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public long QuantityQuantity { get; set; }
    }

    public partial class Prices
    {
        [JsonProperty("cost")]
        public double? Cost { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public double? CompareAtPrice { get; set; }
    }

    public class order_assign_payload
    {
        [JsonPropertyName("order_id")]
        public string? order_id { get; set; }

        [JsonPropertyName("store_no")]
        public string? store_no { get; set; }
    }

    public partial class CourierResponse
    {
        [JsonProperty("AirwayBillNumber")]
        public string? AirwayBillNumber { get; set; }

        [JsonProperty("Code")]
        public long? Code { get; set; }

        [JsonProperty("Description")]
        public string? Description { get; set; }

        [JsonProperty("DestinationCode")]
        public string? DestinationCode { get; set; }
    }

    public partial class OrderAddressUpdate
    {
        [JsonProperty("newPhoneNo")]
        public string? NewPhoneNo { get; set; } = "";

        [JsonProperty("newCity")]
        public string? NewCity { get; set; } = "";

        [JsonProperty("newCountry")]
        public string? NewCountry { get; set; } = "";

        [JsonProperty("newAddress")]
        public string? NewAddress { get; set; } = "";

        [JsonProperty("cancelReason")]
        public string? CancelReason { get; set; } = "";
    }

    public class OrderCountsDto
    {
        public long AllOrders { get; set; }
        public long AssignedOrders { get; set; }
        public long AcceptedOrders { get; set; }
        public long DispatchedOrders { get; set; }
        public long PostedOrders { get; set; }
        public long CancelledOrders { get; set; }
        public long ErrorOrders { get; set; }
    }
}