using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Omni_Courier_Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        private async Task<bool> LoadConfigurations()
        {
            MongoClient mongoDbClient = new MongoClient(ScopeVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(ScopeVariables.MongoDatabase);
            var Configurations = mongoDB.GetCollection<BsonDocument>("plugin_config");
            var filter = $@"{{""platform"":""RetailPro"",""platform_version"":""2.2""}}";
            var configResult = await Configurations.Find(filter).ToListAsync();


            var courierCollection = mongoDB.GetCollection<BsonDocument>("courier");
            var document = await courierCollection.Find("{is_enabled:true}").FirstOrDefaultAsync();
            if (document != null)
            {
                var courierName = document["courier_name"]?.ToString()?.ToLower() ?? "";
                var isEnabled = bool.Parse(document["is_enabled"]?.ToString() ?? "false");

                if (courierName == "imile")
                    ScopeVariables.iMile_isActive = isEnabled;

                if (courierName == "c3x")
                    ScopeVariables.C3X_isActive = isEnabled;
            }
          

            if (configResult.Any())
            {
                var configObject = configResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var configurations = JsonConvert.DeserializeObject<List<RetailProConfigurationInfo>>(JsonConvert.SerializeObject(configObject))?.FirstOrDefault();

                ScopeVariables.RProConfig = configurations;

                ScopeVariables.OracleConnectionString = "User Id=reportuser;Password=report;" +
                "Data Source=(DESCRIPTION=" +
                $"(ADDRESS=(PROTOCOL=TCP)(HOST={ScopeVariables.RProConfig?.ServerAddress.ToString()})(PORT=1521))" +
                "(CONNECT_DATA=(SERVICE_NAME=rproods.prism)));" +
                "Pooling=true;Min Pool Size=5;Max Pool Size=50;";
            }


            return true;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = await LoadConfigurations();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Omni Courier Service running, time now is: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(36000, stoppingToken);
            }
        }
    }

    public class EventNode
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string __typename { get; set; }

        // BasicEvent
        public string Action { get; set; }
        public string SubjectId { get; set; }
        public string SubjectType { get; set; }

        // CommentEvent
        public string RawMessage { get; set; }
    }
}
