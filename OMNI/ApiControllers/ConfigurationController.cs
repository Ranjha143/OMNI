using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using OMNI;
using PluginManager;
using System.Net;


namespace OMNI_Dashboard.ApiControllers
{
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IMongoCollection<RetailProConfigurationInfo> retailproConfigCollection;
        private readonly IMongoCollection<ShopifyConfigurationInfo> shopifyConfigCollection;
        private readonly IMongoCollection<ServiceConfigurationInfo> servicesCollection;
        private readonly MongoClient mongoDbClient = new(Globals.MongoConnectionString);

        ConfigurationController() {

            var mongoDB = mongoDbClient.GetDatabase(Globals.MongoDatabase);

            retailproConfigCollection = mongoDB.GetCollection<RetailProConfigurationInfo>("omni_configurations");
            shopifyConfigCollection = mongoDB.GetCollection<ShopifyConfigurationInfo>("omni_configurations");
            servicesCollection = mongoDB.GetCollection<ServiceConfigurationInfo>("omni_services");
        }


        [HttpPost]
        [Route("api/v1/configuration")]
        public async Task<IActionResult> SaveConfiguration(Configurations configurationInfo)
        {
            try
            { /*
                var configuration = new List<Configurations>();

                RetailProConfigurationInfo? retailproConfig = configurationInfo.retailpro;
                ShopifyConfigurationInfo? shopifyConfig = configurationInfo.shopify;
                ServiceConfigurationInfo? serviceConfig = configurationInfo.service;

               
                if (configurationInfo.client_name != null)
                {
                    var rpFilter = $@"{{""client_name"":""{configurationInfo.client_name}"", ""platform"":""{retailproConfig.platform}"" }}";
                    var rpConfigResult = await retailproConfigCollection.Find(rpFilter).ToListAsync();

                    if (rpConfigResult.Any())
                    {

                        var rpUpdate = Builders<RetailProConfigurationInfo>.Update
                                       .Set("platform_version", retailproConfig.platform_version)
                                       .Set("db_user_name", retailproConfig.db_user_name)
                                       .Set("db_password", retailproConfig.db_password)
                                       .Set("server_address", retailproConfig.server_address)
                                       .Set("db_server_port", retailproConfig.db_server_port)
                                       .Set("db_sid", retailproConfig.db_sid);

                        await retailproConfigCollection.UpdateManyAsync(rpFilter, rpUpdate);
                    }
                    else
                    {
                        await retailproConfigCollection.InsertOneAsync(retailproConfig);
                    }



                    var shopifyFilter = $@"{{""client_name"":""{configurationInfo.client_name}"", ""platform"":""{shopifyConfig.platform}"" }}";
                    var shopifyConfigResult = await retailproConfigCollection.Find(shopifyFilter).ToListAsync();
                    if (shopifyConfigResult.Any())
                    {
                        var rpUpdate = Builders<ShopifyConfigurationInfo>.Update
                                       .Set("store_identifier", shopifyConfig.store_identifier)
                                       .Set("location_name", shopifyConfig.location_name)
                                       .Set("api_key", shopifyConfig.api_access_token)
                                       .Set("api_access_token", shopifyConfig.api_access_token);

                        await shopifyConfigCollection.UpdateManyAsync(shopifyFilter, rpUpdate);
                    }
                    else
                    {
                        await shopifyConfigCollection.InsertOneAsync(shopifyConfig);
                    }


                    var serviceFilter = $@"{{""client_name"":""{configurationInfo.client_name}""}}";
                    var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
                    if (serviceResult.Any())
                    {


                        var orderService = serviceResult.Where(s=>s.service == "Order").FirstOrDefault();
                        if (orderService != null) {

                            var srvc_filter = $@"{{""client_name"":""{configurationInfo.client_name}"",""service"":""Order""}}";
                            var srvc_update = Builders<ServiceConfigurationInfo>.Update
                                       .Set("enabled", configurationInfo.order_service);
                            await servicesCollection.UpdateManyAsync(srvc_filter, srvc_update);
                        }

                        var invService = serviceResult.Where(s => s.service == "Inventory").FirstOrDefault();
                        if (invService != null)
                        {
                            var srvc_filter = $@"{{""client_name"":""{configurationInfo.client_name}"",""service"":""Inventory""}}";
                            var srvc_update = Builders<ServiceConfigurationInfo>.Update
                                .Set("enabled", configurationInfo.inventory_service);
                            await servicesCollection.UpdateManyAsync(srvc_filter, srvc_update);
                        }

                    }
                    else
                    {
                       
                        await servicesCollection.InsertOneAsync(new ServiceConfigurationInfo
                        {
                            Client_name = configurationInfo.client_name,
                            service = "Inventory",
                            enabled = configurationInfo.inventory_service
                        });

                        await servicesCollection.InsertOneAsync(new ServiceConfigurationInfo
                        {
                            Client_name = configurationInfo.client_name,
                            service = "Order",
                            enabled = configurationInfo.order_service
                        });
                    }


                }
                */
                return StatusCode((int)HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });

            }
        }

        [HttpGet]
        [Route("api/v1/configuration")]
        public async Task<IActionResult> GetConfiguration(string client_name)
        {
            try
            {
                /*
                var retailProConfig = retailproConfigCollection.Find(r => r.client_name == client_name && r.platform == "RetailPro").FirstOrDefault();
                var ShopifyConfig = shopifyConfigCollection.Find(r => r.ClientName == client_name && r.Platform == "Shopify").FirstOrDefault();
                var ServiceList = servicesCollection.Find(s => s.Client_name == client_name).ToList();
 
                return StatusCode((int)HttpStatusCode.OK, new { retaipro = JsonConvert.SerializeObject(retailProConfig), shopify = JsonConvert.SerializeObject(ShopifyConfig), service_status = JsonConvert.SerializeObject(ServiceList) });
               */


                return StatusCode((int)HttpStatusCode.BadRequest, new {  });
            }
            catch (Exception ex)
            {

                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });

            }

        }
    }
}

