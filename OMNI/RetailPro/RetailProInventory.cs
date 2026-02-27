using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMNI_Dashboard.ApiControllers;
using PluginManager;
using Quartz;
using RetailPro2_X.BL;
using System.ComponentModel;

namespace RetailPro2_X
{
    internal class RetailProInventory : IJob
    {
        private readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public RetailProInventory()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //var workerTask = Task.Factory.StartNew(() => loadConfigurations().Wait());
            //Task.WaitAll(workerTask);

            if (!GlobalVariables.RetailProInventoryWorker && !GlobalVariables.ShopifyInventoryWorker)  // 
            {
                GlobalVariables.RetailProInventoryWorker = true;
                await Task.Delay(0);
                threadWorker.RunWorkerAsync();
            }
        }

        //private async Task<bool> loadConfigurations()
        //{
        //    MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
        //    var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
        //    IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");
        //    var serviceFilter = $@"{{""service"":""Inventory""}}";
        //    var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
        //    if (serviceResult.Any())
        //    {
        //        var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
        //        var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault();

        //        GlobalVariables.InventoryServiceIsEnabled = ServiceInfo?.Enabled ?? false;
        //    }
        //    return true;
        //}

        private void ThreadWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var GetRetailProProductsTask = Task.Factory.StartNew(() => GetRetailProProducts().Wait());
            GetRetailProProductsTask.Wait();

            var GetInventoryTask = Task.Factory.StartNew(() => GetInventory().Wait());
            GetInventoryTask.Wait();
        }

        private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
        }

        private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            GlobalVariables.RetailProInventoryWorker = false;
        }

        private async Task<bool> GetRetailProProducts()
        {
            DateTime? lastFetchDateTime = null;
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            var productCollection = mongoDB.GetCollection<BsonDocument>("products");
            var productSCollection = mongoDB.GetCollection<BsonDocument>("products_s");
            var productNewCollection = mongoDB.GetCollection<BsonDocument>("products_new");

            var logCollection = mongoDB.GetCollection<BsonDocument>("retailpro_product_log");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            //if (GlobalVar.iables.InventoryServiceIsEnabled)
            //{}
                try
                {
                    var logFilter = "{type:'products'}"; //Builders<BsonDocument>.Filter.Eq("log_date", BsonValue.Create(DateTime.Today));
                    var logResult = await logCollection.Find(logFilter).ToListAsync();
                    var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                    if (log == null)
                    {
                        BsonDocument document = new BsonDocument();
                        document["type"] = "products";
                        document["created_at"] = DateTime.Now;
                        document["last_updated_at"] = DateTime.Now;
                        document["log_date"] = DateTime.Today;
                        document["inventory_count"] = 0;
                        document["processed"] = 0;
                        document["failed"] = 0;
                        await logCollection.InsertOneAsync(document);
                        lastFetchDateTime = DateTime.Now;
                    }
                    else
                    {
                        lastFetchDateTime = DateTime.Parse(log["last_updated_at"]?.ToString() ?? DateTime.Now.ToString()).AddMinutes(-15);
                    }

                    var RProConfig = GlobalVariables.RProConfig;

                    var startTime = DateTime.Now;

                    var skip = 0;
                    var fetch = 5000;
                    var hasRows = true;
                    var inventoryCount = 0;

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
                                        inner join RPS.STORE ST on ST.SID = IQ.STORE_SID and i.sbs_sid=st.sbs_sid
                                        where
                                        I.Text3 = 'WEB'
                                        AND S.SBS_NO = {RProConfig.SBS_NO}
                                        AND ST.ACTIVE = 1
                                        AND I.ACTIVE = 1
                                        AND ST.STORE_NO IN ({GlobalVariables.RProConfig.InventoryStores})
                                        AND NVL(I.ORDERABLE, 0) = 1
                            ";
                    List<StoreQuantities> storeWiseQuantities = ADO.ReadAsync<StoreQuantities>(quantityQuery);

                    var priceLevels = GlobalVariables.RProConfig.PriceLevels;
                    var priceQuery = $@"
                                    SELECT * FROM (
                                    select
                                    TO_CHAR(I.SID) AS SID,
                                    I.alu AS ALU,
                                    I.alu AS SKU,
                                    I.Cost as Cost,
                                    NVL(P1.PRICE, 0) AS PRICE,
                                    PL.PRICE_LVL
                                    FROM RPS.INVN_SBS_ITEM I
                                    INNER JOIN RPS.PRICE_LEVEL PL ON I.SBS_SID = PL.SBS_SID
                                    LEFT JOIN RPS.INVN_SBS_PRICE P1 ON I.SID = P1.INVN_SBS_ITEM_SID
                                            AND I.SBS_SID = P1.SBS_SID
                                            AND PL.SID = P1.PRICE_LVL_SID
                                    WHERE 1=1
                                    AND I.SBS_SID = (select sid from RPS.Subsidiary where sbs_no = {RProConfig.SBS_NO})
                                    AND I.Active = 1
                                    AND I.Text3 = 'WEB'
                                    AND NVL(I.ORDERABLE, 0) = 1
                                    ) PIVOT (
                                        SUM(PRICE) FOR PRICE_LVL IN ({priceLevels.Price} AS price, {priceLevels.CompareAtPrice} AS compareAtPrice)
                                    )
                                ";

                    List<PriceLevelInfo> PriceLevelInfoList = ADO.ReadAsync<PriceLevelInfo>(priceQuery);

                    while (hasRows)
                    {
                        var itemQuery = $@"
                        SELECT
                            TO_CHAR(I.SID) AS SID,
                            TO_CHAR(I.CREATED_DATETIME, 'DD-MON-YYYY HH24:MI:SS') AS CREATED_DATETIME,
                            TO_CHAR(I.style_sid) AS style_sid,
                            I.ALU AS ALU,
                            I.ALU AS SKU,
                            I.UPC AS UPC,
                            I.Cost AS Cost,
                            I.Description1,
                            I.Description2,
                            I.Description3,
                            I.Description4,
                            I.ATTRIBUTE AS ATTRIBUTE,
                            I.ITEM_SIZE AS ITEM_SIZE,
                            I.LONG_DESCRIPTION AS Long_Description,
                            TO_CHAR(NVL(I.KIT_TYPE, 0)) AS KIT_TYPE
                        FROM RPS.INVN_SBS_ITEM I
                        WHERE I.SBS_SID = (SELECT SID FROM RPS.Subsidiary WHERE sbs_no = {RProConfig.SBS_NO})
                            AND I.Active = 1
                            AND I.Text3 = 'WEB'
                            AND NVL(I.ORDERABLE, 0) = 1
                        ORDER BY I.CREATED_DATETIME
                        OFFSET {skip} ROWS
                        FETCH NEXT {fetch} ROWS ONLY
                    ";

                        List<InventoryModel> itemList = ADO.ReadAsync<InventoryModel>(itemQuery);
                        hasRows = itemList.Count() > 0;

                        if (hasRows)
                        {
                            skip += fetch;

                            var bulkOps = new List<WriteModel<BsonDocument>>();
                            foreach (var doc in itemList)
                            {
                                var qty = storeWiseQuantities.Where(s => s.SID == doc.SID).ToList();
                                doc.COMPANY_OH = qty.Sum(s => s.QTY);
                                doc.STORE_QUANTITIES = qty;

                                var priceObject = PriceLevelInfoList.Where(p => p.SID == doc.SID).ToList();
                                if (priceObject != null)
                                {
                                    doc.PRICES = new Prices
                                    {
                                        Cost = priceObject.FirstOrDefault()?.cost ?? 0,
                                        Price = priceObject.FirstOrDefault()?.price ?? 0,
                                        CompareAtPrice = priceObject.FirstOrDefault()?.compareAtPrice ?? 0
                                    };
                                }

                                var filter = Builders<BsonDocument>.Filter.Eq("sid", doc.SID);
                                var document = BsonDocument.Parse(JsonConvert.SerializeObject(doc));
                                var replaceOne = new ReplaceOneModel<BsonDocument>(filter, document) { IsUpsert = true };
                                bulkOps.Add(replaceOne);
                            }

                            await productSCollection.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
                        }
                    }

                    /*
                    await mongoDB.DropCollectionAsync("products_s");

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
                            AND I.Text3 = 'WEB'
                            AND NVL(I.ORDERABLE, 0) = 1
                           --  AND i.MODIFIED_DATETIME > to_date('{lastFetchDateTime?.ToString("dd-MMM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)}','DD-MON-YYYY HH:MI:SS AM')
                            Order by I.CREATED_DATETIME
                            OFFSET {skip} ROWS
                            FETCH NEXT {fetch} ROWS ONLY
                        ";

                        List<InventoryModel> itemList = ADO.ReadAsync<InventoryModel>(itemQuery);
                        hasRows = itemList.Count() > 0;

                        if (hasRows)
                        {
                            skip += fetch;

                            var bulkOps = new List<WriteModel<BsonDocument>>();
                            foreach (var doc in itemList)
                            {
                                var qty = storeWiseQuantities.Where(s => s.SID == doc.SID).ToList();
                                var combinedQty = qty.Sum(s => s.QTY);
                                doc.COMPANY_OH = combinedQty;
                                doc.STORE_QUANTITIES = qty;
                                var priceObject = PriceLevelInfoList.Where(p => p.SID == doc.SID).ToList();
                                if (priceObject != null)
                                {
                                    doc.PRICES = new Prices
                                    {
                                        Cost = priceObject.FirstOrDefault()?.cost ?? 0,
                                        Price = priceObject.FirstOrDefault()?.price ?? 0,
                                        CompareAtPrice = priceObject.FirstOrDefault()?.compareAtPrice ?? 0
                                    };
                                }

                                var filter = Builders<BsonDocument>.Filter.Eq("sid", doc.SID);
                                var document = BsonDocument.Parse(JsonConvert.SerializeObject(doc));
                                var replaceOne = new ReplaceOneModel<BsonDocument>(filter, document) { IsUpsert = true };
                                bulkOps.Add(replaceOne);
                            }

                            var bulkResult = productSCollection.BulkWrite(bulkOps);
                        }
                    }
                    */
                    ///=============================================   NEW ITEMS

                    var NewItemPipeline = new BsonDocument[]
                      {
                            new BsonDocument("$lookup", new BsonDocument
                            {
                                { "from", "products" },
                                { "localField", "sid" },
                                { "foreignField", "sid" },
                                { "as", "inventory_o" }
                            }),
                            new BsonDocument("$unwind", new BsonDocument
                            {
                                { "path", "$inventory_o" },
                                { "preserveNullAndEmptyArrays", true }
                            }),
                            new BsonDocument("$match", new BsonDocument
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

                    var mongoInsertManyOptions = new InsertManyOptions { IsOrdered = false };
                    if (NewItems.Count > 0)
                    {
                        await productNewCollection.InsertManyAsync(NewItems, mongoInsertManyOptions);
                    }

                    await mongoDB.DropCollectionAsync("products");
                    productCollection.InsertManyAsync(await productSCollection.Find("{}").ToListAsync(), mongoInsertManyOptions).Wait();

                    lastFetchDateTime = DateTime.Now;
                    var timelaps = (DateTime.Now - startTime).TotalMinutes;

                    var update = Builders<BsonDocument>.Update
                        .Set("last_updated_at", BsonDateTime.Create(lastFetchDateTime))
                        .Set("inventory_style_count", inventoryCount)
                        .Set("inventory_item_count", inventoryCount)
                        .Set("inventory_processed", inventoryCount)
                        .Set("total_time_elapsed", Math.Round(timelaps, 2) + " Min");
                    await logCollection.UpdateOneAsync(logFilter, update);
                }
                catch (Exception ex)
                {
                    BsonDocument document = new BsonDocument();
                    document["service"] = "RetailPro Inventory Pull Service";
                    document["created_at"] = DateTime.Now;
                    document["exception_message"] = ex.Message;
                    document["exception_source"] = ex.Source;
                    document["exception_stack_trace"] = ex.StackTrace;
                    document["seen"] = false;
                    await exceptionCollection.InsertOneAsync(document);
                }
            

            return true;
        }

        private static async Task<bool> GetInventory()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            var inventory = mongoDB.GetCollection<BsonDocument>("inventory");
            var logCollection = mongoDB.GetCollection<BsonDocument>("retailpro_inventory_log");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            //if (GlobalVariables.InventoryServiceIsEnabled)
            //{ }
                try
                {
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

                    while (hasRows)
                    {
                        var priceLevels = GlobalVariables.RProConfig.PriceLevels; //.Select(s => Convert.ToUInt64(s)).ToList();
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
                                       WHERE IX.{GlobalVariables.RProConfig.ItemSearchkey} = I.{GlobalVariables.RProConfig.ItemSearchkey}
                                            AND S.STORE_NO IN ({GlobalVariables.RProConfig.InventoryStores})
                                            AND S.Active = 1
                                            AND IX.SBS_SID = (SELECT SID FROM RPS.Subsidiary WHERE sbs_no = {GlobalVariables.RProConfig.SBS_NO})
                                       GROUP BY IX.{GlobalVariables.RProConfig.ItemSearchkey}
                                    ) AS STORE_OH,
                                    PL.PRICE_LVL
                                FROM RPS.INVN_SBS_ITEM I
                                INNER JOIN RPS.SUBSIDIARY SS ON I.SBS_SID = SS.SID
                                INNER JOIN RPS.PRICE_LEVEL PL ON I.SBS_SID = PL.SBS_SID
                                LEFT JOIN RPS.INVN_SBS_PRICE P1 ON I.SID = P1.INVN_SBS_ITEM_SID
                                    AND I.SBS_SID = P1.SBS_SID
                                    AND PL.SID = P1.PRICE_LVL_SID
                                WHERE SS.SBS_NO = {GlobalVariables.RProConfig.SBS_NO}
                                    AND I.Active = 1
                                    AND I.Text3 = 'WEB'
                                    AND NVL(I.ORDERABLE, 0) = 1
                            ) PIVOT (
                                SUM(PRICE) FOR PRICE_LVL IN ({priceLevels.Price} AS price, {priceLevels.CompareAtPrice} AS compareAtPrice)
                            )
                            OFFSET {skip} ROWS
                            FETCH NEXT {fetch} ROWS ONLY
                        ";

                        List<PriceLevelInfo> itemList = ADO.ReadAsync<PriceLevelInfo>(itemQuery);
                        hasRows = itemList.Count() > 0;

                        if (hasRows)
                        {
                            skip += fetch;

                            var bulkOps = itemList.Select(item =>
                            {
                                var json = JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore
                                });

                                var doc = BsonDocument.Parse(json);
                                var filter = Builders<BsonDocument>.Filter.Eq("alu", doc["alu"]);
                                var update = new BsonDocument("$set", doc);
                                return new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };
                            }).ToList();

                            await inventory.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
                        }
                    }

                    var timelaps = (DateTime.Now - startTime).TotalMinutes;

                    var update = Builders<BsonDocument>.Update
                        .Set("last_updated_at", BsonDateTime.Create(DateTime.Now))
                        .Set("inventory_style_count", inventoryCount)
                        .Set("inventory_item_count", inventoryCount)
                        .Set("inventory_processed", inventoryCount)
                        .Set("total_time_elapsed", Math.Round(timelaps, 2) + " Min");
                    await logCollection.UpdateOneAsync(logFilter, update);
                }
                catch (Exception ex)
                {
                    BsonDocument document = new BsonDocument();
                    document["service"] = "RetailPro Inventory Pull Service";
                    document["created_at"] = DateTime.Now;
                    document["exception_message"] = ex.Message;
                    document["exception_source"] = ex.Source;
                    document["exception_stack_trace"] = ex.StackTrace;
                    document["seen"] = false;
                    await exceptionCollection.InsertOneAsync(document);
                }
           

            return true;
        }

        private async Task<bool> UpdatePrice()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");
            var inventory = mongoDB.GetCollection<BsonDocument>("inventory");
            try
            {
                //>>=============================================   NEW Prices

                var priceMergePipeline = new[]
                      {
                            new BsonDocument("$lookup", new BsonDocument
                            {
                                { "from", "shopify_inventory" },
                                { "localField", "alu" },
                                { "foreignField", "variantList.sku" },
                                { "as", "shopifyMatch" }
                            }),
                            new BsonDocument("$unwind", "$shopifyMatch"),
                            new BsonDocument("$unwind", "$shopifyMatch.variantList"),
                            new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$alu", "$shopifyMatch.variantList.sku" }),
                                new BsonDocument("$or", new BsonArray
                                {
                                    new BsonDocument("$ne", new BsonArray { "$price", "$shopifyMatch.variantList.price" }),
                                    new BsonDocument("$ne", new BsonArray { "$compareAtPrice", "$shopifyMatch.variantList.compareAtPrice" })
                                })
                            }))),
                            new BsonDocument("$project", new BsonDocument
                            {
                                { "_id", 0 },
                                { "productId", "$shopifyMatch.productId" },
                                { "productGId", "$shopifyMatch.id" },
                                { "variantGId", "$shopifyMatch.variantList.id" },
                                { "alu", "$shopifyMatch.variantList.sku" },
                                { "cost", "$cost" },
                                { "price", "$price" },
                                { "compareAtPrice", "$compareAtPrice" }
                            }),
                            new BsonDocument("$merge", new BsonDocument
                            {
                                { "into", "inv_price_new" },
                                { "on", "_id" },
                                { "whenMatched", "merge" },
                                { "whenNotMatched", "insert" }
                            })
                        };

                var priceDifference = (await inventory.AggregateAsync<BsonDocument>(priceMergePipeline)).ToList();
                //>>=============================================   NEW Prices
            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Shopify Update Price Outer Exception ";
                document["created_at"] = DateTime.Now;
                document["sku_alu"] = "";
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }
            return true;
        }

        private async Task<bool> UpdateQuantity()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");
            var inventory = mongoDB.GetCollection<BsonDocument>("inventory");
            try
            {
                var pipeline = new[]
  {
                    // --- $lookup ---
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "shopify_inventory" },
                        { "localField", "alu" },
                        { "foreignField", "variantList.sku" },
                        { "as", "shopifyMatch" }
                    }),

                    // --- $unwind shopifyMatch ---
                    new BsonDocument("$unwind", "$shopifyMatch"),

                    // --- $unwind shopifyMatch.variantList ---
                    new BsonDocument("$unwind", "$shopifyMatch.variantList"),

                    // --- $match ---
                    new BsonDocument("$match", new BsonDocument
                    {
                        { "$expr", new BsonDocument("$and", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$alu", "$shopifyMatch.variantList.sku" }),
                                new BsonDocument("$ne", new BsonArray { "$store_oh", "$shopifyMatch.variantList.inventoryQuantity" })
                            })
                        }
                    }),

                    // --- $addFields ---
                    new BsonDocument("$addFields", new BsonDocument
                    {
                        { "store_oh", new BsonDocument("$cond", new BsonDocument
                            {
                                { "if", new BsonDocument("$lt", new BsonArray { "$store_oh", 0 }) },
                                { "then", 0 },
                                { "else", "$store_oh" }
                            })
                        }
                    }),

                    // --- $project ---
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 0 },
                        { "alu", 1 },
                        { "Quantity", "$store_oh" }
                    }),

                    // --- $merge ---
                    new BsonDocument("$merge", new BsonDocument
                    {
                        { "into", "inv_qty_new" },
                        { "on", "alu" },
                        { "whenMatched", "merge" },
                        { "whenNotMatched", "insert" }
                    })
                };
                var result = inventory.Aggregate<BsonDocument>(pipeline).ToList();
            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Shopify Update Quantity";
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private static async Task<bool> UpdateProduct()
        {
            try
            {
                MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
                var productCollection = mongoDB.GetCollection<BsonDocument>("products");
                var inventoryCollection = mongoDB.GetCollection<BsonDocument>("inventory");

                /*
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
                using (var cursor = await productCollection.AggregateAsync<BsonDocument>(pipeline, options))
                {
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
                }

                //var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("alu");
                //var indexOptions = new CreateIndexOptions { Unique = true, Name = "unique_alu_index" };
                //var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, indexOptions);
                //await productCollection.Indexes.CreateOneAsync(indexModel);
                */

                var pipeline2 = new[]
                   {
                        // 1️⃣ Match documents with null prices
                        new BsonDocument("$match", new BsonDocument("prices", BsonNull.Value)),

                        // 2️⃣ Lookup inventory by 'alu'
                        new BsonDocument("$lookup", new BsonDocument
                        {
                            { "from", "inventory" },
                            { "localField", "alu" },
                            { "foreignField", "alu" },
                            { "as", "inv" }
                        }),

                        // 3️⃣ Unwind inventory matches
                        new BsonDocument("$unwind", new BsonDocument
                        {
                            { "path", "$inv" },
                            { "preserveNullAndEmptyArrays", false }
                        }),

                        // 4️⃣ Set prices from inventory
                        new BsonDocument("$set", new BsonDocument("prices", new BsonDocument
                        {
                            { "cost", "$inv.cost" },
                            { "price", "$inv.price" },
                            { "compareAtPrice", "$inv.compareAtPrice" }
                        })),

                        // 5️⃣ Merge back into products by 'alu'
                        new BsonDocument("$merge", new BsonDocument
                        {
                            { "into", "products" },
                            { "on", "alu" },
                            { "whenMatched", "merge" },
                            { "whenNotMatched", "discard" }
                        })
                    };

                var result = await productCollection.AggregateAsync<BsonDocument>(pipeline2);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class InvCount
    {
        public long STYLES { get; set; }
        public long ITEMS { get; set; }
    }

    public class InventoryModel
    {
        [JsonProperty("sid")]
        public string SID { get; set; } = "";

        [JsonProperty("sbs_no")]
        public int SBS_NO { get; set; }

        [JsonProperty("created_datetime")]
        public string CREATED_DATETIME { get; set; } = "";

        [JsonProperty("style_sid")]
        public string STYLE_SID { get; set; } = "";

        [JsonProperty("alu")]
        public string ALU { get; set; } = "";

        [JsonProperty("sku")]
        public string SKU { get; set; } = "";

        [JsonProperty("upc")]
        public string UPC { get; set; } = "";

        [JsonProperty("description1")]
        public string DESCRIPTION1 { get; set; } = "";

        [JsonProperty("description2")]
        public string DESCRIPTION2 { get; set; } = "";

        [JsonProperty("description3")]
        public string DESCRIPTION3 { get; set; } = "";

        [JsonProperty("description4")]
        public string DESCRIPTION4 { get; set; } = "";

        [JsonProperty("attribute")]
        public string ATTRIBUTE { get; set; } = "";

        [JsonProperty("item_size")]
        public string ITEM_SIZE { get; set; } = "";

        [JsonProperty("long_description")]
        public string LONG_DESCRIPTION { get; set; } = "";

        [JsonProperty("kit_type")]
        public int KIT_TYPE { get; set; }

        [JsonProperty("cost")]
        public decimal? COST { get; set; }

        [JsonProperty("store_oh")]
        public long? STORE_OH { get; set; }

        [JsonProperty("company_oh")]
        public long? COMPANY_OH { get; set; }

        [JsonProperty("prices")]
        public Prices? PRICES { get; set; }

        [JsonProperty("has_qty")]
        public bool HasQty { get; set; }

        [JsonProperty("store_quantities")]
        public List<StoreQuantities>? STORE_QUANTITIES { get; set; }
    }

    //public class StoreQuantities
    //{
    //    [JsonProperty("sid")]
    //    public string SID { get; set; }

    //    [JsonProperty("alu")]
    //    public string ALU { get; set; }

    //    [JsonProperty("store_no")]
    //    public string STORE_NO { get; set; }

    //    [JsonProperty("store_name")]
    //    public string STORE_NAME { get; set; }

    //    [JsonProperty("qty")]
    //    public long QTY { get; set; }

    //}

    //public partial class PriceInfo
    //{
    //    [JsonProperty("currency")]
    //    public string Currency { get; set; } = "";

    //    [JsonProperty("price_lvl_name")]
    //    public string PriceLvlName { get; set; } = "";

    //    [JsonProperty("price_lvl")]
    //    public long PriceLvl { get; set; }

    //    [JsonProperty("price_lvl_sid")]
    //    public string PriceLvlSid { get; set; } = "";

    //    [JsonProperty("price")]
    //    public decimal? Price { get; set; }
    //}

    public partial class PriceCollection
    {
        [JsonProperty("upc")]
        public string Upc { get; set; } = "";

        [JsonProperty("alu")]
        public string Alu { get; set; } = "";

        [JsonProperty("price")]
        public List<Price> Price { get; set; } = [];
    }

    public partial class Price
    {
        [JsonProperty("price_level")]
        public long PriceLevel { get; set; }

        [JsonProperty("price")]
        public double PricePrice { get; set; }
    }
}