using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;
using RetailPro2_X;
using Shopify;

namespace OMNI
{
    public class StartupConfigLoader : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await LoadShopifyConfig();
            await LoadRetailProConfig();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task LoadShopifyConfig()
        {
            try
            {

                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                var Configurations = mongoDB.GetCollection<BsonDocument>("plugin_config");
                var configResult = await Configurations.Find($@"{{""platform"":""Shopify""}}").ToListAsync();
                var configObject = configResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);

                GlobalVariables.ShopifyConfig = JsonConvert.DeserializeObject<List<ShopifyConfigurationInfo>>(JsonConvert.SerializeObject(configObject))?.FirstOrDefault();

                if (GlobalVariables.ShopifyConfig != null)
                {

                    GlobalVariables.ShopifyConfig.GraphUrl = $"https://{GlobalVariables.ShopifyConfig?.StoreIdentifier}.myshopify.com/admin/api/{GlobalVariables.ShopifyConfig?.PlatformVersion}/graphql.json";
                    IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("services");
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

                }
            }
            catch (Exception)
            {
            }
        }

        private async Task LoadRetailProConfig()
        {
            try
            {
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

                    if (GlobalVariables.RProConfig != null)
                    {

                        GlobalVariables.OracleConnectionString =
                            $"User Id=reportuser;Password=report;" +
                            $"Data Source=(DESCRIPTION=" +
                            $"(ADDRESS=(PROTOCOL=TCP)(HOST={GlobalVariables.RProConfig?.ServerAddress.ToString()})(PORT=1521))" +
                            $"(CONNECT_DATA=(SERVICE_NAME=rproods.prism)));" +
                            "Pooling=true;Min Pool Size=5;Max Pool Size=50;";

                        GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig?.PrismUser ?? "", GlobalVariables.RProConfig?.PrismPassword ?? "", "webclient") ?? "";

                        IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("services");

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
                }
            }
            catch (Exception)
            {

            }
        }
    }

}
