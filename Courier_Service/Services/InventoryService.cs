using Dapper;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniCommon;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni_Courier_Service.Services
{
    public class InventoryService
    {

        private readonly ILogger<InventoryService> _logger;
        readonly IMongoDatabase mongoDB;
        readonly IMongoCollection<BsonDocument> servicesCollection;
        readonly IMongoCollection<BsonDocument> productCollection;
        readonly IMongoCollection<BsonDocument> productSCollection;
        readonly IMongoCollection<BsonDocument> productNewCollection;
        readonly IMongoCollection<BsonDocument> logCollection;
        readonly IMongoCollection<BsonDocument> exceptionCollection;
        readonly IMongoCollection<BsonDocument> inventoryCollection;
        readonly IMongoCollection<BsonDocument> inventory_sCollection;
        readonly IMongoCollection<BsonDocument> inv_price_newCollection;
        readonly IMongoCollection<BsonDocument> inv_qty_newCollection;
        readonly OracleConnection retailProConnection;
        public InventoryService(IMongoClient mongoClient, IConfiguration configuration, ILogger<InventoryService> logger)
        {
            _logger = logger;
            var OmniDB = configuration["MongoDB:DataBase"];

            mongoDB = mongoClient.GetDatabase(OmniDB);
            servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");
            productCollection = mongoDB.GetCollection<BsonDocument>("products");
            productSCollection = mongoDB.GetCollection<BsonDocument>("products_s");
            productNewCollection = mongoDB.GetCollection<BsonDocument>("products_new");

            //logCollection = mongoDB.GetCollection<BsonDocument>("retailpro_product_log");
            exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            inventoryCollection = mongoDB.GetCollection<BsonDocument>("inventory");
            inventory_sCollection = mongoDB.GetCollection<BsonDocument>("inventory_s");
            inv_price_newCollection = mongoDB.GetCollection<BsonDocument>("inv_price_new");
            inv_qty_newCollection = mongoDB.GetCollection<BsonDocument>("inv_qty_new");

            retailProConnection = new OracleConnection(ScopeVariables.OracleConnectionString);
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("InventoryProcessor started at {time}", DateTimeOffset.Now);

            try
            {
                var result1 = await GetRetailProProducts();
                if (!result1) _logger.LogWarning("GetRetailProProducts failed");

                var result2 = await GetInventory();
                if (!result2) _logger.LogWarning("GetInventory failed");

                var result3 = await UpdateProduct();
                if (!result3) _logger.LogWarning("UpdateProduct failed");

                _logger.LogDebug("All steps completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing inventory.");
            }

            _logger.LogInformation("InventoryProcessor finished at {time}", DateTimeOffset.Now);
        }

        private async Task<bool> GetRetailProProducts()
        {
            DateTime? lastFetchDateTime = null;

            try
            {
                var logFilter = "{type:'products'}"; //Builders<BsonDocument>.Filter.Eq("log_date", BsonValue.Create(DateTime.Today));
                var logResult = await logCollection.Find(logFilter).ToListAsync();
                var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                if (log == null)
                {
                    BsonDocument document = new()
                    {
                        ["type"] = "products",
                        ["created_at"] = DateTime.Now,
                        ["last_updated_at"] = DateTime.Now,
                        ["log_date"] = DateTime.Today,
                        ["inventory_count"] = 0,
                        ["processed"] = 0,
                        ["failed"] = 0
                    };
                    await logCollection.InsertOneAsync(document);
                    lastFetchDateTime = DateTime.Now;
                }
                else
                {
                    lastFetchDateTime = DateTime.Parse(log["last_updated_at"]?.ToString() ?? DateTime.Now.ToString()).AddMinutes(-15);
                }

                var RProConfig = ScopeVariables.RProConfig;

                var startTime = DateTime.Now;

                var skip = 0;
                var fetch = 5000;
                var hasRows = true;
                var inventoryCount = 0;

                await mongoDB.DropCollectionAsync("products_s");
                var mongoInsertManyOptions = new InsertManyOptions { IsOrdered = false };

                while (hasRows)
                {
                    var itemQuery = $@"
                            select 
                            TO_CHAR(I.SID) AS SID, 
                            to_char(I.CREATED_DATETIME, 'DD-MON-YYYY HH24:MI:SS') as CREATED_DATETIME,
                            to_CHAR(I.style_sid) as style_sid,
                            I.ALU as ALU,
                            I.ALU as SKU,
                            I.UPC as UPC,
                            I.Cost as Cost,
                            I.Description1,
                            I.Description2,
                            I.Description3,
                            I.Description4,
                            I.ATTRIBUTE as ATTRIBUTE,
                            I.ITEM_SIZE AS ITEM_SIZE,
                            I.LONG_DESCRIPTION AS Long_Description,
                            to_char(NVL(I.KIT_TYPE,0)) as KIT_TYPE
                            FROM RPS.INVN_SBS_ITEM I
                            WHERE 1=1
                            AND I.SBS_SID = (select sid from RPS.Subsidiary where sbs_no = {RProConfig.SbsNo})
                            AND I.Active = 1 
                            AND lower(I.{RProConfig.ProductSyncFlagField}) = '{RProConfig.ProductSyncFlagValue.ToLower()}'
                            AND NVL(I.ORDERABLE, 0) = 1
                            AND i.MODIFIED_DATETIME > to_date('{lastFetchDateTime?.ToString("dd-MMM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}','DD-MON-YYYY HH:MI:SS AM')
                            Order by I.CREATED_DATETIME
                            OFFSET {skip} ROWS
                            FETCH NEXT {fetch} ROWS ONLY
                            ";
                    List<InventoryModel> itemList = (await retailProConnection.QueryAsync<InventoryModel>(itemQuery)).ToList();

                    hasRows = itemList.Count > 0;
                    var storeList = string.Join(",", RProConfig.StoreWiseQuantity.Select(s => s.InventoryStore).ToList());
                    var quantityQuery = $@"
                            select
                            TO_CHAR(I.SID) AS SID,
                            I.ALU as ALU,
                            ST.store_no as store_no,
                            ST.store_name as store_name,
                            greatest(NVL(IQ.QTY,0),0) as Qty
                            FROM RPS.INVN_SBS_ITEM I
                            inner join RPS.INVN_SBS_ITEM_QTY IQ ON I.SID = IQ.INVN_SBS_ITEM_SID AND I.SBS_SID = IQ.SBS_SID
                            INNER JOIN RPS.SUBSIDIARY S ON I.SBS_SID = S.SID
                            inner join RPS.STORE ST on ST.SID = IQ.STORE_SID
                            where 
                            AND lower(I.{RProConfig.ProductSyncFlagField}) = '{RProConfig.ProductSyncFlagValue.ToLower()}'
                            AND i.MODIFIED_DATETIME > to_date('{lastFetchDateTime?.ToString("dd-MMM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}','DD-MON-YYYY HH:MI:SS AM')
                            AND S.SBS_NO = {RProConfig.SbsNo}
                            AND ST.ACTIVE = 1
                            AND I.ACTIVE = 1
                            AND ST.store_no in ({storeList})
                            AND NVL(I.ORDERABLE, 0) = 1
                            ";
                    List<StoreQuantities> storeWiseQuantities = (await retailProConnection.QueryAsync<StoreQuantities>(quantityQuery)).ToList();

                    foreach (var storeQty in storeWiseQuantities)
                    {
                        var storethreashold = RProConfig.StoreWiseQuantity.Where(s => s.InventoryStore == storeQty.STORE_NO).FirstOrDefault();
                        if (storethreashold != null)
                        {
                            if (storethreashold.QuantityThreasholdType == "%")
                            {
                                storeQty.QTY = storeQty.QTY - (int)(storeQty.QTY * (storethreashold.QuantityThreasholdValue / 100.0));
                            }
                            else if (storethreashold.QuantityThreasholdType == "val")
                            {
                                storeQty.QTY = storeQty.QTY - (int)storethreashold.QuantityThreasholdValue;
                            }

                            if (storeQty.QTY < 0) storeQty.QTY = 0;
                        }
                    }

                    if (hasRows)
                    {
                        skip += fetch;

                        var bulkOps = new List<WriteModel<BsonDocument>>();
                        foreach (var doc in itemList)
                        {
                            var StoreQuantities = storeWiseQuantities.Where(s => s.SID == doc.SID).OrderBy(s=>s.STORE_NAME).ToList();
                            var combinedQty = StoreQuantities.Sum(s => s.QTY);

                            doc.COMPANY_OH = combinedQty;
                            doc.STORE_QUANTITIES = StoreQuantities;

                            var filter = Builders<BsonDocument>.Filter.Eq("sid", doc.SID);
                            var document = BsonDocument.Parse(JsonConvert.SerializeObject(doc));
                            var replaceOne = new ReplaceOneModel<BsonDocument>(filter, document) { IsUpsert = true };
                            bulkOps.Add(replaceOne);
                        }

                        // Execute bulk upsert
                        var bulkResult = productSCollection.BulkWrite(bulkOps);
                        var bulkResultw = productCollection.BulkWrite(bulkOps);

                        foreach (var item in storeWiseQuantities)
                        {
                            var filter = Builders<BsonDocument>.Filter.Eq("sid", item.SID);

                            // Step 1: Try positional update
                            var qty_update = Builders<BsonDocument>.Update.Set("store_quantities.$[elem].qty", item.QTY);

                            var arrayFilters = new List<ArrayFilterDefinition>
                                {
                                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                        new BsonDocument
                                        {
                                            { "elem.sid", item.SID },
                                            { "elem.store_no", item.STORE_NO }
                                        }
                                    )
                                };

                            var options = new UpdateOptions { ArrayFilters = arrayFilters };
                            var result = await productCollection.UpdateOneAsync(filter, qty_update, options);

                            // Step 2: If not matched (no array or matching element), push new entry
                            if (result.MatchedCount > 0 && result.ModifiedCount > 0)
                            {
                                // Successfully updated
                                _logger.LogInformation("Updated quantity for SID: {SID}, Store No: {STORE_NO}", item.SID, item.STORE_NO);
                            }
                            else
                            {
                                var itemInfo = await productCollection.Find(filter).FirstOrDefaultAsync();
                                var store_quantities = itemInfo?["store_quantities"]?.AsBsonArray ?? [];
                                if (itemInfo != null && store_quantities.Count == 0)
                                {
                                    var storeSidQuery = $@"select to_char(sid) as sid, store_name from rps.store where store_no = {item.STORE_NO} ";

                                    List<JObject> storeSIdObj = (await retailProConnection.QueryAsync<JObject>(quantityQuery)).ToList();


                                    var store_name = storeSIdObj[0]["STORE_NAME"]?.ToString();

                                    var pushUpdate = Builders<BsonDocument>.Update.Push("store_quantities",
                                        new BsonDocument
                                        {
                                            { "sid", item.SID },
                                            { "alu", item.ALU },
                                            { "store_no", item.STORE_NO },
                                            { "store_name", store_name },
                                            { "qty", item.QTY }
                                        });

                                    await productCollection.UpdateOneAsync(filter, pushUpdate);
                                }
                            }

                        }
                    }
                }

                ///=============================================   NEW ITEMS

                var NewItemPipeline = new BsonDocument[]
                {
                    new("$lookup", new BsonDocument
                    {
                        { "from", "products" },
                        { "localField", "sid" },
                        { "foreignField", "sid" },
                        { "as", "inventory_o" }
                    }),
                    new("$unwind", new BsonDocument
                    {
                        { "path", "$inventory_o" },
                        { "preserveNullAndEmptyArrays", true }
                    }),
                    new("$match", new BsonDocument
                    {
                        { "$expr", new BsonDocument
                            {
                                { "$ne", new BsonArray { "$sid", "$inventory_o.sid" } }
                            }
                        }
                    })
                };

                var NewItems = await productSCollection.Aggregate<BsonDocument>(NewItemPipeline).ToListAsync();

                NewItems.ForEach(i =>
                {
                    i.Remove("inventory_o");
                    i.Remove("matched_doc");
                });

                if (NewItems.Count > 0)
                {
                    await productNewCollection.InsertManyAsync(NewItems, mongoInsertManyOptions);
                    await productCollection.InsertManyAsync(NewItems, mongoInsertManyOptions);
                }

                lastFetchDateTime = DateTime.Now;
                var timelaps = (DateTime.Now - startTime).TotalMinutes;

                _logger.LogInformation(
                   "Inventory sync completed at {LastUpdatedAt} and it took {ElapsedMinutes} min to complete",
                   lastFetchDateTime,
                   Math.Round(timelaps, 2)
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Get Retail Pro Products");
                return false;
            }


        }

        private async Task<bool> GetInventory()
        {

            try
            {
                var RProConfig = ScopeVariables.RProConfig;

                var logFilter = "{type:'inventory'}"; //Builders<BsonDocument>.Filter.Eq("log_date", BsonValue.Create(DateTime.Today));
                var logResult = await logCollection.Find(logFilter).ToListAsync();
                var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                if (log == null)
                {
                    BsonDocument document = new BsonDocument();
                    document["type"] = "inventory";
                    document["created_at"] = DateTime.Now;
                    document["last_updated_at"] = DateTime.Now;
                    document["log_date"] = DateTime.Today;
                    document["inventory_count"] = 0;
                    document["processed"] = 0;
                    document["failed"] = 0;
                    await logCollection.InsertOneAsync(document);
                }

                var startTime = DateTime.Now;

                var skip = 0;
                var fetch = 5000;
                var hasRows = true;
                var inventoryCount = 0;

                var mongoInsertManyOptions = new InsertManyOptions { IsOrdered = false };

                await mongoDB.DropCollectionAsync("inventory_s");
                while (hasRows)
                {
                    var priceLevels = ScopeVariables.RProConfig.PriceLevels; //.Select(s => Convert.ToUInt64(s)).ToList();
                    /*
                    var itemQuery = $@"
                            SELECT UPC, ALU, COST, STORE_OH, price, compareAtPrice
                            FROM (
                                SELECT 
                                    I.UPC,
                                    I.ALU,
                                    I.cost AS COST, 
                                    NVL(P1.PRICE, 0) AS PRICE,
                                    (
                                        SELECT SUM(NVL(IQ.QTY, 0)) AS OH_QTY
                                        FROM RPS.INVN_SBS_ITEM IX 
                                        INNER JOIN RPS.STORE S ON IX.SBS_SID = S.SBS_SID
                                        LEFT JOIN RPS.INVN_SBS_ITEM_QTY IQ ON IX.SBS_SID = IQ.SBS_SID 
                                            AND IX.SID = IQ.INVN_SBS_ITEM_SID 
                                            AND S.SID = IQ.STORE_SID
                                       WHERE IX.{ScopeVariables.RProConfig.ItemSearchkey} = I.{ScopeVariables.RProConfig.ItemSearchkey}
                                            AND S.STORE_NO IN ({ScopeVariables.RProConfig.InventoryStores})
                                            AND S.Active = 1
                                        GROUP BY IX.{ScopeVariables.RProConfig.ItemSearchkey}
                                    ) AS STORE_OH,
                                    PL.PRICE_LVL 
                                FROM RPS.INVN_SBS_ITEM I  
                                INNER JOIN RPS.SUBSIDIARY SS ON I.SBS_SID = SS.SID
                                INNER JOIN RPS.PRICE_LEVEL PL ON I.SBS_SID = PL.SBS_SID
                                LEFT JOIN RPS.INVN_SBS_PRICE P1 ON I.SID = P1.INVN_SBS_ITEM_SID 
                                    AND I.SBS_SID = P1.SBS_SID 
                                    AND PL.SID = P1.PRICE_LVL_SID
                                WHERE SS.SBS_NO = {ScopeVariables.RProConfig.SbsNo}
                                    AND I.Active = 1 
                                    AND lower(I.{RProConfig.ProductSyncFlagField}) = '{RProConfig.ProductSyncFlagValue.ToLower()}'
                                    AND NVL(I.ORDERABLE, 0) = 1
                            ) PIVOT (
                                SUM(PRICE) FOR PRICE_LVL IN ({priceLevels?.Price} AS price, {priceLevels?.CompareAtPrice} AS compareAtPrice)
                            )
                            OFFSET {skip} ROWS
                            FETCH NEXT {fetch} ROWS ONLY
                        ";
                    */

                    var itemQuery = $@"

                            SELECT UPC, ALU, COST, STORE_OH, price, compareAtPrice
                            FROM (
                                SELECT 
                                    I.UPC,
                                    I.ALU,
                                    I.cost AS COST, 
                                    NVL(P1.PRICE, 0) AS PRICE,
                                    PL.PRICE_LVL 
                                FROM RPS.INVN_SBS_ITEM I  
                                INNER JOIN RPS.SUBSIDIARY SS ON I.SBS_SID = SS.SID
                                INNER JOIN RPS.PRICE_LEVEL PL ON I.SBS_SID = PL.SBS_SID
                                LEFT JOIN RPS.INVN_SBS_PRICE P1 ON I.SID = P1.INVN_SBS_ITEM_SID 
                                    AND I.SBS_SID = P1.SBS_SID 
                                    AND PL.SID = P1.PRICE_LVL_SID
                                WHERE SS.SBS_NO = {ScopeVariables.RProConfig.SbsNo}
                                    AND I.Active = 1 
                                    AND lower(I.{RProConfig.ProductSyncFlagField}) = '{RProConfig.ProductSyncFlagValue.ToLower()}'
                                    AND NVL(I.ORDERABLE, 0) = 1
                            ) PIVOT (
                                SUM(PRICE) FOR PRICE_LVL IN ({priceLevels?.Price} AS price, {priceLevels?.CompareAtPrice} AS compareAtPrice)
                            )
                            OFFSET {skip} ROWS
                            FETCH NEXT {fetch} ROWS ONLY
                        ";

                    List<PriceLevelInfo> itemList = (await retailProConnection.QueryAsync<PriceLevelInfo>(itemQuery)).ToList();
                    hasRows = itemList.Count() > 0;

                    if (hasRows)
                    {
                        skip += fetch;

                        var jsonList = itemList.Select(p => JsonConvert.SerializeObject(p, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))
                               .Select(json => BsonDocument.Parse(json))
                               .ToList();
                        await inventory_sCollection.InsertManyAsync(jsonList, mongoInsertManyOptions);
                    }
                }

                ///=============================================   NEW Prices
                var PriceComparisonPipeline = new[]
                {
                        new BsonDocument
                        {
                            { "$lookup", new BsonDocument
                                {
                                    { "from", "inventory" },
                                    { "localField", ScopeVariables.RProConfig.ItemSearchkey.ToLower() },
                                    { "foreignField", ScopeVariables.RProConfig.ItemSearchkey.ToLower() },
                                    { "as", "matched_docs" }
                                }
                            }
                        },
                        new BsonDocument
                        {
                            { "$addFields", new BsonDocument
                                {
                                    { "matched_doc", new BsonDocument
                                        {
                                            { "$arrayElemAt", new BsonArray { "$matched_docs", 0 } }
                                        }
                                    }
                                }
                            }
                        },
                        new BsonDocument
                        {
                            { "$match", new BsonDocument
                                {
                                    { "$or", new BsonArray
                                        {
                                            new BsonDocument
                                            {
                                                { "matched_doc", new BsonDocument { { "$exists", false } } }
                                            },
                                            new BsonDocument
                                            {
                                                { "$expr", new BsonDocument
                                                    {
                                                        { "$ne", new BsonArray { "$cost", "$matched_doc.cost" } }
                                                    }
                                                }
                                            },
                                            new BsonDocument
                                            {
                                                { "$expr", new BsonDocument
                                                    {
                                                        { "$ne", new BsonArray { "$compareAtPrice", "$matched_doc.compareAtPrice" } }
                                                    }
                                                }
                                            },
                                            new BsonDocument
                                            {
                                                { "$expr", new BsonDocument
                                                    {
                                                        { "$ne", new BsonArray { "$price", "$matched_doc.price" } }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new BsonDocument
                        {
                            { "$project", new BsonDocument
                                {
                                    {"_id", 0 },
                                    { ScopeVariables.RProConfig.ItemSearchkey.ToLower(), 1 },
                                    { "cost", 1 },
                                    { "price", 1 },
                                    { "compareAtPrice", 1 }
                                }
                            }
                        }
                };


                var priceDifferenceSet = await inventory_sCollection.Aggregate<BsonDocument>(PriceComparisonPipeline).ToListAsync();
                if (priceDifferenceSet.Count > 0)
                {
                    var bulkOps = new List<WriteModel<BsonDocument>>();
                    foreach (var doc in priceDifferenceSet)
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq($"{ScopeVariables.RProConfig.ItemSearchkey.ToLower()}", doc[$"{ScopeVariables.RProConfig.ItemSearchkey.ToLower()}"]);
                        var replaceOne = new ReplaceOneModel<BsonDocument>(filter, doc) { IsUpsert = true };
                        bulkOps.Add(replaceOne);
                    }
                    var bulkResult = inv_price_newCollection.BulkWrite(bulkOps);
                }

                ///=============================================   NEW Quantity
                BsonDocument[]? ItemQuantityPipeline = [];

                ItemQuantityPipeline = new[]
                {
                    new BsonDocument
                    {
                        { "$lookup", new BsonDocument
                            {
                                { "from", "inventory" },
                                { "localField", ScopeVariables.RProConfig.ItemSearchkey.ToLower() },
                                { "foreignField", ScopeVariables.RProConfig.ItemSearchkey.ToLower() },
                                { "as", "matched_docs" }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$addFields", new BsonDocument
                            {
                                { "matched_doc", new BsonDocument
                                    {
                                        { "$arrayElemAt", new BsonArray { "$matched_docs", 0 } }
                                    }
                                }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$match", new BsonDocument
                            {
                                { "$or", new BsonArray
                                    {
                                        new BsonDocument
                                        {
                                            { "matched_doc", new BsonDocument { { "$exists", false } } }
                                        },
                                        new BsonDocument
                                        {
                                            { "$expr", new BsonDocument
                                                {
                                                    { "$ne", new BsonArray { "$store_oh", "$matched_doc.store_oh" } }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$project", new BsonDocument
                            {
                                {"_id", 0 },
                                { ScopeVariables.RProConfig.ItemSearchkey.ToLower(), 1 },
                                { "Quantity", "$store_oh" },
                            }
                        }
                    }
                };

                var quantityDifferenceSet = await inventory_sCollection.Aggregate<BsonDocument>(ItemQuantityPipeline).ToListAsync();

                if (quantityDifferenceSet.Count > 0)
                {

                    var bulkOps = new List<WriteModel<BsonDocument>>();
                    foreach (var doc in quantityDifferenceSet)
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq($"{ScopeVariables.RProConfig.ItemSearchkey.ToLower()}", doc[$"{ScopeVariables.RProConfig.ItemSearchkey.ToLower()}"]);
                        var replaceOne = new ReplaceOneModel<BsonDocument>(filter, doc) { IsUpsert = true };
                        bulkOps.Add(replaceOne);
                    }
                    var bulkResult = inv_qty_newCollection.BulkWrite(bulkOps);
                }

                await mongoDB.DropCollectionAsync("inventory");

                var newData = (await inventory_sCollection.FindAsync("{}")).ToList();
                await inventoryCollection.InsertManyAsync(newData, mongoInsertManyOptions);

                var timelaps = (DateTime.Now - startTime).TotalMinutes;

                _logger.LogInformation(
                   "Inventory sync completed at {LastUpdatedAt} and it took {ElapsedMinutes} min to complete",
                   DateTime.Now,
                   Math.Round(timelaps, 2)
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Get Inventory");
                return false;
            }
            
        }

        private async Task<bool> UpdateProduct()
        {

            try
            {
                // Aggregation pipeline with batch processing
                var pipeline = new[]
                {
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "inventory" },
                        { "localField", "alu" },
                        { "foreignField", "alu" },
                        { "as", "inventory" }
                    }),
                    new BsonDocument("$unwind", new BsonDocument
                    {
                        { "path", "$inventory" },
                        { "preserveNullAndEmptyArrays", true }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 1 },
                        { "alu", 1 },
                        { "newCost", "$inventory.cost" },
                        { "newPrice", "$inventory.price" },
                        { "newCompareAtPrice", "$inventory.compareAtPrice" },
                        { "newStoreOh", "$inventory.store_oh" },
                        { "newCombinedOh", "$inventory.multi_store_qty" }
                    })
                };

                var results = await productCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

                // Use an async cursor to stream data in batches
                var options = new AggregateOptions { BatchSize = 10000 }; // Adjust batch size as needed
                using var cursor = await productCollection.AggregateAsync<BsonDocument>(pipeline, options);
                while (await cursor.MoveNextAsync()) // Move to the next batch
                {
                    var batch = cursor.Current; // Get current batch
                    var bulkUpdates = new List<WriteModel<BsonDocument>>();

                    foreach (var doc in batch)
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                        var update = Builders<BsonDocument>.Update
                            .Set("prices", new BsonDocument
                            {
                                    { "cost", doc["newCost"].ToDouble() },
                                    { "price", doc["newPrice"].ToDouble() },
                                    { "compareAtPrice", doc["newCompareAtPrice"].ToDouble() }
                            })
                            .Set("store_oh", doc["newStoreOh"].ToInt32());
                        //.Set("combined_oh", doc["newCombinedOh"].ToInt32());

                        bulkUpdates.Add(new UpdateOneModel<BsonDocument>(filter, update));
                    }

                    // Execute bulk update for the batch
                    if (bulkUpdates.Count > 0)
                    {
                        await productCollection.BulkWriteAsync(bulkUpdates);
                        Console.WriteLine($"Updated {bulkUpdates.Count} records in this batch...");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update Product");
                return false;
            }
        }


    }
}
