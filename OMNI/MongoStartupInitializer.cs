using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PluginManager;

namespace OMNI
{
    public class MongoDbInitializerHostedService : IHostedService
    {
        private readonly ILogger<MongoDbInitializerHostedService> _logger;
        private readonly IMongoDatabase _database;

        public MongoDbInitializerHostedService(ILogger<MongoDbInitializerHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;

            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            _database = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MongoDB initialization started");

            await EnsureCollectionAndIndexAsync("inv_price_new");
            await EnsureCollectionAndIndexAsync("inv_qty_new");
            await EnsureCollectionAndIndexAsync("inventory");

            _logger.LogInformation("MongoDB initialization completed");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task EnsureCollectionAndIndexAsync(string collectionName)
        {
            // Check collection
            if (!await CollectionExistsAsync(collectionName))
            {
                _logger.LogInformation("Creating collection: {Collection}", collectionName);
                await _database.CreateCollectionAsync(collectionName);
            }

            var collection = _database.GetCollection<BsonDocument>(collectionName);

            // Check index
            if (!await IndexExistsAsync(collection, "alu_1"))
            {
                _logger.LogInformation("Creating unique index on {Collection}.alu", collectionName);

                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("alu");
                var indexOptions = new CreateIndexOptions
                {
                    Unique = true,
                    Name = "alu_1"
                };

                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<BsonDocument>(indexKeys, indexOptions));
            }
        }

        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var cursor = await _database.ListCollectionsAsync(
                new ListCollectionsOptions { Filter = filter });

            return await cursor.AnyAsync();
        }

        private async Task<bool> IndexExistsAsync(
            IMongoCollection<BsonDocument> collection,
            string indexName)
        {
            var indexes = await collection.Indexes.ListAsync();
            return (await indexes.ToListAsync())
                .Any(i => i["name"] == indexName);
        }
    }
}