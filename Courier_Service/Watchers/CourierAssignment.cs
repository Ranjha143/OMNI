using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Omni_Courier_Service.Services;

namespace Omni_Courier_Service.Watchers
{
    public class CourierAssignment : BackgroundService
    {
        private readonly ILogger<CourierAssignment> _logger;
        private readonly IMongoDatabase mongoDB;
        private readonly IMongoCollection<BsonDocument> _configurationCollection;
        private readonly IMongoCollection<BsonDocument> _resumeCollection;

        public CourierAssignment(ILogger<CourierAssignment> logger, IMongoClient mongoClient, IConfiguration config)
        {
            _logger = logger;

            var databaseName = config["MongoDB:DataBase"]; // 👈 uses your existing config
            mongoDB = mongoClient.GetDatabase(databaseName);

            _configurationCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
            _resumeCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_ResumeToken");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChangeStreamWorker started at {time}", DateTimeOffset.Now);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
                .Match(change => change.OperationType == ChangeStreamOperationType.Insert ||
                                 change.OperationType == ChangeStreamOperationType.Update ||
                                 change.OperationType == ChangeStreamOperationType.Replace);

            var options = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            var lastTokenDoc = await _resumeCollection.Find(FilterDefinition<BsonDocument>.Empty)
                                                      .Sort(Builders<BsonDocument>.Sort.Descending("_id"))
                                                      .FirstOrDefaultAsync(stoppingToken);

            if (lastTokenDoc?["resumeToken"] is BsonDocument token)
                options.StartAfter = token;

            using var cursor = await _configurationCollection.WatchAsync(pipeline, options, stoppingToken);

            await foreach (var change in cursor.ToAsyncEnumerable())
            {
                try
                {
                    if (change.FullDocument == null)
                    {
                        _logger.LogWarning("FullDocument is null for change on _id={id}", change.DocumentKey["_id"]);
                        continue;
                    }

                    _logger.LogInformation("Detected {op} on _id={id}",
                        change.OperationType,
                        change.FullDocument["_id"]);

                    var documentIsAvailableForCourierAssignment = !bool.Parse(change.FullDocument["is_courier_assigned"]?.ToString() ?? "false") &&
                        !bool.Parse(change.FullDocument["posted"]?.ToString() ?? "false") &&
                        change.FullDocument["accepted_by_store"] == "accepted" &&
                        !bool.Parse(change.FullDocument["dispatched"]?.ToString() ?? "false");

                    if (documentIsAvailableForCourierAssignment)
                    {
                        await AssignCourier(change.FullDocument);
                    }
                       
                    await _resumeCollection.ReplaceOneAsync(
                        FilterDefinition<BsonDocument>.Empty,
                        new BsonDocument { { "resumeToken", change.ResumeToken } },
                        new ReplaceOptions { IsUpsert = true },
                        cancellationToken: stoppingToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing change event");
                }
            }
        }


        private Task AssignCourier(BsonDocument document)
        {
            try
            {
                if (ScopeVariables.iMile_isActive)
                    iMile.AssignCourier(document["order_id"].ToString(), _logger, mongoDB);

                else if (ScopeVariables.C3X_isActive)
                    C3X.AssignCourier(document["order_id"].ToString(), _logger, mongoDB);



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"New item Creation error: {document["_id"]}");
            }

            return Task.CompletedTask;
        }
    }
}
