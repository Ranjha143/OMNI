using GraphQL;
using GraphQL.Validation;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMNI;
using OMNI.Shopify.BusinessLogic;
using PluginManager;
using Quartz;
using Quartz.Impl;
using Shopify;

namespace Shopify
{
    public class ShopifyPlugin
    {
        private readonly MongoClient mongoDbClient;

        public ShopifyPlugin()
        {
            mongoDbClient = new(Globals.MongoConnectionString);
            Globals.mongoDB = mongoDbClient.GetDatabase(Globals.MongoDatabase);
        }

        public void Start()
        {
            //var configTask = Task.Factory.StartNew(() => LoadConfigurations().Wait());
            //configTask.Wait();

#if DEBUG

            GetShopifyInventory();

            GetShopifyOrder();

            //if (GlobalVariables.ShopifyConfig.SaleFulfillmentDirection == "push")
            //    SendShopifyFulfilment();

            //if (GlobalVariables.ShopifyConfig.SaleCancellationDirection == "push")
            //    SendShopifyCancellation();

            //if (GlobalVariables.ShopifyConfig.SaleFulfillmentDirection == "push")
            //    SendShopifyFulfilment();

#endif

#if !DEBUG

            GetShopifyInventory();

            GetShopifyOrder();

            if (GlobalVariables.ShopifyConfig.SaleFulfillmentDirection == "push")
                SendShopifyFulfilment();

            //if (GlobalVariables.ShopifyConfig.SaleCancellationDirection == "push")
            //    SendShopifyCancellation();
#endif
        }

        /*private static async Task<bool> LoadConfigurations()
        {
            
            try
            {
                
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                var Configurations = mongoDB.GetCollection<BsonDocument>("plugin_config");
                var configResult = await Configurations.Find($@"{{""platform"":""Shopify""}}").ToListAsync();
                var configObject = configResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);

                GlobalVariables.ShopifyConfig = JsonConvert.DeserializeObject<List<ShopifyConfigurationInfo>>(JsonConvert.SerializeObject(configObject))?.FirstOrDefault() ?? new();

                GlobalVariables.ShopifyConfig.GraphUrl = $"https://{GlobalVariables.ShopifyConfig?.StoreIdentifier}.myshopify.com/admin/api/{GlobalVariables.ShopifyConfig?.PlatformVersion}/graphql.json";

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

                serviceFilter = $@"{{""service"":""Inventory""}}";
                serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
                if (serviceResult.Count > 0)
                {
                    var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault() ?? new();

                    GlobalVariables.InventoryServiceIsEnabled = ServiceInfo.Enabled;
                    GlobalVariables.InventoryServiceInterval = ServiceInfo.Interval;
                }

                var timeZoneQry = @"
                    query {
                        shop {
                            ianaTimezone
                            timezoneOffset
                        }
                    }";

                var response = await GraphAPI.QueryAsync(timeZoneQry);
                if (response != null && response.Errors == null)
                {
                    var data = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(response.Data));
                    GlobalVariables.ShopifyConfig.Timezone = data["shop"]["ianaTimezone"]?.ToString() ?? "";
                    GlobalVariables.ShopifyConfig.TimezoneOffset = data["shop"]["timezoneOffset"]?.ToString() ?? "";
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
    
        }   */

        private static async void GetShopifyOrder()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            IJobDetail job = JobBuilder.Create<ShopifyOrderGraphQl>()
                .WithIdentity("ShopifyOrderJob", "ShopifyOrder")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ShopifyOrderTrigger", "ShopifyOrder")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(15) //.WithIntervalInMinutes(GlobalVariables.OrderServiceInterval)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }

        //private static async void ShopifyRefunds()
        //{
        //    StdSchedulerFactory factory = new StdSchedulerFactory();
        //    IScheduler scheduler = await factory.GetScheduler();
        //    IJobDetail job = JobBuilder.Create<ShopifyRefund>()
        //        .WithIdentity("ShopifyRefundsJob", "ShopifyRefund")
        //        .Build();
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("ShopifyRefundsTrigger", "ShopifyRefund")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInMinutes(GlobalVariables.OrderServiceInterval)
        //            .RepeatForever())
        //        .Build();
        //    await scheduler.ScheduleJob(job, trigger);
        //    await scheduler.Start();
        //}

        //private static async void GetShopifyCanceled()
        //{
        //    StdSchedulerFactory factory = new StdSchedulerFactory();
        //    IScheduler scheduler = await factory.GetScheduler();
        //    IJobDetail job = JobBuilder.Create<ShopifyCanceled>()
        //        .WithIdentity("ShopifyCanceledJob", "ShopifyCanceled")
        //        .Build();
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("ShopifyCanceledTrigger", "ShopifyCanceled")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInMinutes(GlobalVariables.OrderServiceInterval)
        //            .RepeatForever())
        //        .Build();
        //    await scheduler.ScheduleJob(job, trigger);
        //    await scheduler.Start();
        //}

        private static async void SendShopifyFulfilment()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            IJobDetail job = JobBuilder.Create<FulfillShopifyOrder>()
                .WithIdentity("ShopifyFulfillOrderJob", "ShopifyFulfillOrder")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ShopifyFulfillOrderTrigger", "ShopifyFulfillOrder")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(15) //.WithIntervalInMinutes(GlobalVariables.OrderServiceInterval)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount())
                .Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }

        //private static async void SendShopifyCancellation()
        //{
        //    StdSchedulerFactory factory = new StdSchedulerFactory();
        //    IScheduler scheduler = await factory.GetScheduler();
        //    IJobDetail job = JobBuilder.Create<CancelShopifyOrder>()
        //        .WithIdentity("CancelShopifyOrderJob", "CancelShopifyOrder")
        //        .Build();
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("CancelShopifyOrderTrigger", "CancelShopifyOrder")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInSeconds(15) //.WithIntervalInMinutes(GlobalVariables.OrderServiceInterval)
        //            .RepeatForever()
        //            .WithMisfireHandlingInstructionNextWithRemainingCount())
        //        .Build();
        //    await scheduler.ScheduleJob(job, trigger);
        //    await scheduler.Start();
        //}

        private static async void GetShopifyInventory()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            IJobDetail job = JobBuilder.Create<ShopifyInventory>()
                .WithIdentity("ShopifyInventoryJob", "ShopifyInventory")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ShopifyInventoryTrigger", "ShopifyInventory")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(45) //.WithIntervalInMinutes(GlobalVariables.InventoryServiceInterval)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount())
                .Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }
    }
}