using MongoDB.Bson;
using MongoDB.Driver;
using Omni_Courier_Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// CourierConfigurationWatcher
namespace Omni_Courier_Service.Watchers
{
    internal class CourierConfigurationWatcher : BackgroundService
    {
        private readonly ILogger<CourierConfigurationWatcher> _logger;
        private readonly IMongoDatabase mongoDB;
        private readonly IMongoCollection<BsonDocument> _courierConfigurationCollection;
        private readonly IMongoCollection<BsonDocument> _resumeCollection;

        public CourierConfigurationWatcher(ILogger<CourierConfigurationWatcher> logger, IMongoClient mongoClient, IConfiguration config)
        {
            _logger = logger;

            var databaseName = config["MongoDB:DataBase"];
            mongoDB = mongoClient.GetDatabase(databaseName);

            _courierConfigurationCollection = mongoDB.GetCollection<BsonDocument>("courier");
            _resumeCollection = mongoDB.GetCollection<BsonDocument>("courier_ResumeToken");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CourierChangeStreamWorker started at {time}", DateTimeOffset.Now);

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

            using var cursor = await _courierConfigurationCollection.WatchAsync(pipeline, options, stoppingToken);

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

                    await SetCourierStatus(change.FullDocument);

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


        private Task SetCourierStatus(BsonDocument document)
        {
            try
            {   
                var courierName = document["courier_name"]?.ToString()?.ToLower()??"";
                var isEnabled = bool.Parse(document["is_enabled"]?.ToString() ?? "false");

                if (courierName == "imile")
                    ScopeVariables.iMile_isActive = isEnabled;

                if (courierName == "c3x")
                    ScopeVariables.C3X_isActive = isEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"New item Creation error: {document["_id"]}");
            }

            return Task.CompletedTask;
        }
    }
}
