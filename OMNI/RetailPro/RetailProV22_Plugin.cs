using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;
using OMNI;
using PluginManager;
using Quartz;
using Quartz.Impl;

namespace RetailPro2_X
{
    public class RetailProV22_Plugin
    {
        public string Name => "RetailPro2_X";

        public void Start()
        {
            //var configLoader = Task.Factory.StartNew(() => LoadConfigurations().Wait());
            //configLoader.Wait();

#if DEBUG
            //FetchInventoryService();
            //PostSaleOrder();
#endif
#if !DEBUG

            FetchInventoryService();
            PostSaleOrder();

#endif
        }

        public void Stop()
        {
        }

        private static async void PostSaleOrder()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            IJobDetail job = JobBuilder.Create<SaleOrder>()
                .WithIdentity("SaleOrderSyncJob", "SaleOrder")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("SaleOrderSyncTrigger", "SaleOrder")
                //.StartAt(DateBuilder.FutureDate(1, IntervalUnit.Minute))
                .StartNow()

                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(15) //.WithIntervalInMinutes(5)  // GlobalVariables.OrderServiceInterval
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount())
                .Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }

        private static async void FetchInventoryService()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            IJobDetail job = JobBuilder.Create<RetailProInventory>()
                .WithIdentity("InventoryJob", "Inventory")
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("InventoryTrigger", "Inventory")
                //.StartAt(DateBuilder.FutureDate(1, IntervalUnit.Minute))
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(15) //.WithIntervalInMinutes(5)  // GlobalVariables.InventoryServiceInterval
                    .RepeatForever()
                    .WithMisfireHandlingInstructionNextWithRemainingCount())

                .Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }

        /*
        private async Task<bool> LoadConfigurations()
        {
            //var loadedPluginString = _pluginCache?.GetValue<string>("plugin") ?? "";
            //var plugins = loadedPluginString.Split(',');

            //GlobalVariables.MongoConnectionString = _configCache?.GetValue<string>("mongo_connection_string") ?? "";
            //GlobalVariables.MongoDatabase = _configCache?.GetValue<string>("database_name") ?? "com_pro";

            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            var Configurations = mongoDB.GetCollection<BsonDocument>("plugin_config");
            var filter = $@"{{""platform"":""RetailPro"",""platform_version"":""2.2""}}";
            var configResult = await Configurations.Find(filter).ToListAsync();

            if (configResult.Any())
            {
                var configObject = configResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var configurations = JsonConvert.DeserializeObject<List<RetailProConfigurationInfo>>(JsonConvert.SerializeObject(configObject))?.FirstOrDefault();

                GlobalVariables.RProConfig = configurations;

                GlobalVariables.OracleConnectionString =
                "User Id=reportuser;Password=report;" +
                "Data Source=(DESCRIPTION=" +
                $"(ADDRESS=(PROTOCOL=TCP)(HOST={GlobalVariables.RProConfig?.ServerAddress.ToString()})(PORT=1521))" +
                "(CONNECT_DATA=(SERVICE_NAME=rproods.prism)));" +
                "Pooling=true;Min Pool Size=5;Max Pool Size=50;";



                //$"" +
                //$"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={GlobalVariables.RProConfig?.ServerAddress.ToString()})(PORT={GlobalVariables.RProConfig?.DbPort})))" +
                //$"(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=rproods.prism)));" +
                //$"User Id={GlobalVariables.RProConfig?.DbUserName};" +
                //$"Password={GlobalVariables.RProConfig?.DbPassword};";

                GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig.PrismUser, GlobalVariables.RProConfig.PrismPassword, "webclient");

                IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");

                var serviceFilter = $@"{{""service"":""Order""}}";
                var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
                if (serviceResult.Any())
                {
                    var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault();

                    GlobalVariables.OrderServiceIsEnabled = ServiceInfo?.Enabled ?? false;
                    GlobalVariables.OrderServiceInterval = ServiceInfo?.Interval ?? 5;
                }

                var InvServiceFilter = $@"{{""service"":""Inventory""}}";
                var InvServiceResult = await servicesCollection.Find(InvServiceFilter).ToListAsync();
                if (serviceResult.Any())
                {
                    var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault();

                    GlobalVariables.InventoryServiceIsEnabled = ServiceInfo?.Enabled ?? false;
                    GlobalVariables.InventoryServiceInterval = ServiceInfo?.Interval ?? 5;
                }
            }

            return true;
        }
        */
    }

    //public static partial class GlobalVariables
    //{
    //}

    public class PriceLevel
    {
        [JsonProperty("cost")]
        public double? Cost { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public double? CompareAtPrice { get; set; }
    }

    //public class ServiceConfigurationInfo
    //{
    //    public string client_name { get; set; } = "";

    //    public bool inventory_service { get; set; }
    //    public bool order_service { get; set; }

    //    [JsonProperty("service")]
    //    public string service { get; set; }

    //    [JsonProperty("interval")]
    //    public int interval { get; set; }

    //    [JsonProperty("enabled")]
    //    public bool enabled { get; set; }

    //}
}