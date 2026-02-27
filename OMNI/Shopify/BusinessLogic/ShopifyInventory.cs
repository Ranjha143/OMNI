using GraphQL;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;
using Quartz;
using Shopify.GraphQL;
using System.ComponentModel;

namespace Shopify
{
    [DisallowConcurrentExecution]
    internal class ShopifyInventory : IJob
    {
        private readonly string MongoConnectionString = "";
        private readonly string MongoDatabase = "";

        private readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public ShopifyInventory()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;

            MongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoDatabase = GlobalVariables.MongoDatabase;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //var workerTask = Task.Factory.StartNew(() => loadConfigurations().Wait());
            //Task.WaitAll(workerTask);

            if (!GlobalVariables.ShopifyInventoryWorker)  //&& GlobalVariables.InventoryServiceIsEnabled  && !GlobalVariables.RetailProInventoryWorker
            {
                GlobalVariables.ShopifyInventoryWorker = true;
                await Task.Delay(0);
                threadWorker.RunWorkerAsync();
            }
        }

        //private async Task<bool> loadConfigurations()
        //{
        //    MongoClient mongoDbClient = new MongoClient(MongoConnectionString);
        //    var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
        //    IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");
        //    var serviceFilter = $@"{{""service"":""Inventory""}}";
        //    var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
        //    if (serviceResult.Any())
        //    {
        //        var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
        //        var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault() ?? new();

        //        GlobalVariables.InventoryServiceIsEnabled = ServiceInfo.Enabled;
        //    }

        //    return true;
        //}

        private void ThreadWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var getInvTask = Task.Factory.StartNew(() => GetShopifyInventory().Wait());
            Task.WaitAll(getInvTask);

            var MatchInventoryTask = Task.Factory.StartNew(() => MatchInventory().Wait());
            Task.WaitAll(MatchInventoryTask);

            /*
            var newProdTask = Task.Factory.StartNew(() => NewProducts().Wait());
            Task.WaitAll(newProdTask);
            */

            var UpdatePriceTask = Task.Factory.StartNew(() => UpdatePrice().Wait());
            Task.WaitAll(UpdatePriceTask);

            var UpdateQuantityTask = Task.Factory.StartNew(() => UpdateQuantity().Wait());
            Task.WaitAll(UpdateQuantityTask);

            //var changeSetTask = Task.Factory.StartNew(() => GetShopifyInventoryChanges().Wait());
            //Task.WaitAll(changeSetTask);
        }

        private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
        }

        private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            GlobalVariables.ShopifyInventoryWorker = false;
        }

        private async Task<bool> GetShopifyInventory()
        {
            DateTime lastFetchDateTime = DateTime.Now;
            long totalInventory = 0;
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
            var itemCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            var logCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("type", "Shopify Inventory");
                var logResult = await logCollection.Find(filter).ToListAsync();
                var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault() ?? null;

                long lastShopifyId = 0;
                if (log == null)
                {
                    var productCountQty = @"query {productsCount(query: ""id:>=0"") {count}}";
                    var countResponse = await GraphAPI.QueryAsync(productCountQty);
                    if (countResponse != null && countResponse.Errors == null)
                    {
                        var countJson = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(countResponse?.Data));
                        _ = long.TryParse(countJson?["productsCount"]?["count"]?.ToString() ?? "0", out totalInventory);
                    }

                    BsonDocument document = new()
                    {
                        ["type"] = "Shopify Inventory",
                        ["created_at"] = DateTime.Now,
                        ["last_updated_at"] = DateTime.Now,
                        ["total_online_products"] = totalInventory,
                        //document["total_Variants"] = variantCount;
                        ["total_omni_inventory"] = 0
                    };
                    await logCollection.InsertOneAsync(document);
                    lastFetchDateTime = DateTime.Now;
                }
                else
                {
                    _ = DateTime.TryParse(log["last_updated_at"]?.ToString(), out lastFetchDateTime);
                    _ = long.TryParse(log["last_inventory_id_fetched"]?.ToString(), out lastShopifyId);
                }

                bool hasNextRecord = true;
                string endCursor = string.Empty;
                int productCount = 50;
                var createdAt = lastFetchDateTime.AddMinutes(-3).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
                while (hasNextRecord)
                {
                    var param = @$"first: {productCount},query:""""{(string.IsNullOrEmpty(endCursor) ? "" : $",after:\"{endCursor}\"")}";  // updated_at:>'{createdAt}' AND status:'active'
                    var qry = GraphQuery.Product(param);
                    var invResponse = await GraphAPI.QueryAsync(qry);

                    if (invResponse != null)
                    {
                        var JsonResult = JsonConvert.SerializeObject(invResponse.Data);
                        var productdata = JsonConvert.DeserializeObject<ProductResponce>(JsonResult);
                        var productList = productdata?.Products.Edges.Select(p => p.Node).ToList() ?? [];

                        if (productList.Count > 0)
                        {
                            foreach (var product in productList)
                            {
                                _ = long.TryParse(product.Id.Split('/').Last(), out long ProductId);

                                if (ProductId > 0) lastShopifyId = ProductId;

                                var locallyExistingProductQuery = Builders<BsonDocument>.Filter.Eq("productId", ProductId);
                                var exist = await itemCollection.Find(locallyExistingProductQuery).AnyAsync();
                                // if (exist){}else{}

                                product.ProductId = ProductId;
                                product.VariantList.AddRange(product?.Variants?.Edges.Select(v => v.Node) ?? []);
                                // product.Variants = null;

                                var skuListString = "'" + string.Join("' OR sku:'", product?.VariantList.Select(s => s.Sku).ToList() ?? []) + "'";
                                var invLevelQry = @$"
                                      query inventoryItems {{
                                          inventoryItems(first: 200, query: ""sku:{skuListString}"") {{
                                            edges {{
                                              node {{
                                                id
                                                tracked
                                                sku
                                                inventoryLevels(first:100)
                                                {{
                                                    edges{{
                                                        node{{
                                                            location
                                                            {{
                                                                id
                                                                name
                                                            }}
                                                        }}
                                                    }}
                                                }}
                                              }}
                                            }}
                                          }}
                                        }}
                                    ";

                                var invRes = (await GraphAPI.QueryAsync(invLevelQry))?.Data;
                                var locationsResponse = JsonConvert.DeserializeObject<InventoryItemModel>(JsonConvert.SerializeObject(invRes));

                                foreach (var item in product?.VariantList ?? [])
                                {
                                    var invLocations = locationsResponse?.InventoryItems.Edges.Select(s => s.Node).Where(s => s.Sku == item.Sku).ToList();
                                    item.Quantities = item.InventoryItem.InventoryLevels.Edges.FirstOrDefault()?.Node?.Quantities ?? [];

                                    var location = invLocations?.FirstOrDefault()?.InventoryLevels.Edges.Select(s => s.Node).Select(n => n.Location).ToList();
                                    item.InventoryId = invLocations?.FirstOrDefault()?.Id;
                                    item.Locations = location ?? [];
                                }

                                BsonDocument bsonDocument = BsonDocument.Parse(JsonConvert.SerializeObject(product, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                                var resulr = await itemCollection.ReplaceOneAsync(
                                    Builders<BsonDocument>.Filter.Eq("productId", product.ProductId),
                                    bsonDocument, new ReplaceOptions { IsUpsert = true }
                                );
                            }

                            hasNextRecord = productdata?.Products.PageInfo.HasNextPage ?? false;
                            endCursor = productdata?.Products.PageInfo.EndCursor ?? "";
                        }
                        else
                        {
                            hasNextRecord = false;
                        }
                    }
                }
                var updateFilter = "{type:'Shopify Inventory'}";
                var update = Builders<BsonDocument>.Update
                   .Set("last_updated_at", DateTime.Now)
                   .Set("last_inventory_id_fetched", lastShopifyId);
                await logCollection.UpdateOneAsync(updateFilter, update);
            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Fetch Shopify Inventory";
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private async Task<bool> MatchInventory()
        {
            DateTime lastFetchDateTime = DateTime.Now;
            //long totalInventory = 0;
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
            var shopifyProductsCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            var productCollection = mongoDB.GetCollection<BsonDocument>("products");

            var logCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {
                var filter = @$"{{""style_sid"":{{$exists:false}}}}";

                var unmatchedProdResult = await shopifyProductsCollection.Find(filter).ToListAsync();
                var unmatchedObject = unmatchedProdResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var unmatchedList = JsonConvert.DeserializeObject<List<UnmatchedDocuments>>(JsonConvert.SerializeObject(unmatchedObject)) ?? [];

                foreach (var prod in unmatchedList)
                {
                    var skuList = "'" + string.Join("','", prod.VariantList.Select(s => s.Sku).ToList()) + "'";
                    var fltr = @$"{{sku:{{ $in:[{skuList}]}}}}";

                    var toMatchResult = await productCollection.Find(fltr).ToListAsync();
                    var toMatchObject = toMatchResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var toMatcgProd = JsonConvert.DeserializeObject<List<ProductModel>>(JsonConvert.SerializeObject(toMatchObject))?.FirstOrDefault();

                    if (toMatcgProd != null)
                    {
                        var styleSid = toMatcgProd.STYLE_SID;

                        var updateFilter = @$"{{""variantList.sku"":""{toMatcgProd.ALU}""}}";
                        var update = Builders<BsonDocument>.Update.Set("style_sid", styleSid);
                        var result = await shopifyProductsCollection.UpdateManyAsync(updateFilter, update);
                    }
                }

                var documentResult = await shopifyProductsCollection.Find("{matched:false}").ToListAsync();
                var documentsObject = documentResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var documents = JsonConvert.DeserializeObject<List<ProductsNode>>(JsonConvert.SerializeObject(documentsObject)) ?? [];

                foreach (var document in documents)
               {
                    bool allHaveStyleSid = document.StyleSid != null;
                    var update = Builders<BsonDocument>.Update.Set("matched", document.StyleSid != null);
                    await shopifyProductsCollection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("productId", document.ProductId), update);
                }
            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Fetch Shopify Inventory";
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private async Task<bool> NewProducts()
        {
            var startTime = DateTime.Now;
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);

            var logCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
            var shopifyProductsCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");

            var newProductsCollection = mongoDB.GetCollection<BsonDocument>("products_new");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {
                var prodListResult = (await newProductsCollection.FindAsync("{}")).ToList();
                var prodListObject = prodListResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);

                var prodList = JsonConvert.DeserializeObject<List<ProductModel>>(JsonConvert.SerializeObject(prodListObject))?.ToList() ?? null;

                if (prodList != null && prodList.Count > 0)
                {
                    var uniqueStyleList = prodList.Select(s => s.STYLE_SID).Distinct().ToList();

                    foreach (var style in uniqueStyleList)
                    {
                        var styleList = prodList.Where(s => s.STYLE_SID == style).ToList();
                        var existingItemResult = await shopifyProductsCollection.Find(@$"{{style_sid:""{style}""}}").ToListAsync();

                        if (existingItemResult != null && existingItemResult.Count > 0)
                        {
                            var existingObject = existingItemResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                            var existingJson = JsonConvert.SerializeObject(existingObject);
                            var existingProduct = JsonConvert.DeserializeObject<List<ProductsNode>>(existingJson)?.FirstOrDefault() ?? null;
                            var res = await ShopifyProduct.AddVariants(existingProduct?.Id ?? "", styleList, mongoDB);
                            if (res)
                                newProductsCollection.DeleteMany(@$"{{style_sid:""{styleList.Select(s => s.STYLE_SID).FirstOrDefault()}""}}");
                        }
                        else
                        {
                            var res = await ShopifyProduct.CreateNew(styleList, mongoDB);
                            if (res)
                                newProductsCollection.DeleteMany(@$"{{style_sid:""{styleList.Select(s => s.STYLE_SID).FirstOrDefault()}""}}");
                        }
                    }
                }
                var timelaps = (DateTime.Now - startTime).TotalMinutes;
            }
            catch (CustomException ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Shopify create product";
                document["created_at"] = DateTime.Now;
                document["sku_alu"] = ex.ALU;
                //document["order_no"] = ex.OrderNo;
                document["style_sid"] = ex.StyleSid;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Shopify create product";
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

        private async Task<bool> UpdatePrice()
        {
            var startTime = DateTime.Now;
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
            var productCollection = mongoDB.GetCollection<BsonDocument>("products");
            var logCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
            var shopifyProductCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            var inv_price_new = mongoDB.GetCollection<BsonDocument>("inv_price_new");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            var shopify_inventory = mongoDB.GetCollection<BsonDocument>("shopify_inventory");

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
                        { "on", "alu" },
                        { "whenMatched", "merge" },
                        { "whenNotMatched", "insert" }
                    })
                };

                var priceDifference = (await inventory.AggregateAsync<BsonDocument>(priceMergePipeline)).ToList();
                //>>=============================================   NEW Prices

                var priceListFilter = "{}";
                var priceListResult = (await inv_price_new.FindAsync(priceListFilter)).ToList();
                var priceListObject = priceListResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var priceList = JsonConvert.DeserializeObject<List<ProductPrice>>(JsonConvert.SerializeObject(priceListObject))?.ToList() ?? null;

                if (priceList != null && priceList.Count > 0)
                {
                    var distinctProducts = priceList.Select(p => p.productId).Distinct().ToList();

                    foreach (var id in distinctProducts)
                    {
                        try
                        {
                            var variantsToUpdate = priceList.Where(p => p.productId == id).ToList();
                            var productId = variantsToUpdate.FirstOrDefault()?.productGId ?? $"gid://shopify/Product/{id}";
                            var update = ConvertToVariantUpdates(variantsToUpdate);
                            var response = await UpdateVariantPricesAsync(productId, update);
                            if (response != null) {
                                if (response.Errors == null)
                                {
                                    foreach (var item in variantsToUpdate)
                                    {
                                        var searchKeyValue = GlobalVariables.ShopifyConfig?.ItemSearchKey?.ToLower() == "alu" ? item.ALU : item.UPC;

                                        var filter = Builders<BsonDocument>.Filter.ElemMatch<BsonDocument>("variantList", Builders<BsonDocument>.Filter.Eq("sku", item.ALU));
                                        var projection = Builders<BsonDocument>.Projection.Include("variantList").Exclude("_id");
                                        var resultDoc = shopifyProductCollection.Find(filter).Project<BsonDocument>(projection).FirstOrDefault();
                                        if (resultDoc != null)
                                        {
                                            var variants = resultDoc["variantList"].AsBsonArray;

                                            var variantToUpdate = variants.FirstOrDefault(v => v["sku"] == item.ALU);
                                            if (variantToUpdate != null)
                                            {
                                                string inventoryId = variantToUpdate["id"].AsString;
                                                //decimal.TryParse(variantToUpdate["inventoryItem"]["unitCost"]["amount"].ToString(), out decimal cost);
                                                //if (cost != item?.Cost)
                                                //{}
                                                if (item?.Cost > 0)
                                                {
                                                    List<ProductVariant> inventoryIds = new List<ProductVariant> { new ProductVariant() { Id = inventoryId, Sku = item?.ALU ?? "" } };
                                                    await ShopifyProduct.UpdateVariantsCost(inventoryIds, mongoDB);
                                                }
                                            }
                                        }

                                        var updateFilter = Builders<BsonDocument>.Filter.Eq("alu", item?.ALU ?? "");

                                        var update2 = Builders<BsonDocument>.Update
                                             .Set("variantList.$[elem].price", item?.Price ?? 0) // Updating price
                                             .Set("variantList.$[elem].compareAtPrice", item?.CompareAtPrice ?? 0) // Updating compareAtPrice
                                             .Set("variantList.$[elem].inventoryItem.unitCost.amount", item?.Cost); // Updating unitCost.amount

                                        var arrayFilters = new List<ArrayFilterDefinition>
                                        {
                                            new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                                new BsonDocument("elem.inventoryItem.unitCost.amount", new BsonDocument("$exists", true))
                                            )
                                        };

                                        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters, IsUpsert = true };
                                        var result1 = shopifyProductCollection.UpdateMany(filter, update2, updateOptions);

                                        await inv_price_new.DeleteManyAsync(@$"{{{GlobalVariables.ShopifyConfig?.ItemSearchKey}:""{searchKeyValue}""}}");

                                        BsonDocument document = new BsonDocument();
                                        document["type"] = "Shopify Price Update";
                                        document["created_at"] = DateTime.Now;
                                        document["sku_alu"] = item?.ALU;
                                        document["price"] = item?.Price?.ToString("0.00");
                                        document["compareAtPrice"] = item?.CompareAtPrice?.ToString("0.00");
                                        await logCollection.InsertOneAsync(document);
                                    }
                                }
                                else
                                {
                                    foreach (var err in response.Errors)
                                    {
                                        BsonDocument document = new BsonDocument();
                                        document["service"] = "Shopify Update Price";
                                        document["created_at"] = DateTime.Now;
                                        document["product_id"] = id;
                                        document["exception_message"] = err.Message;
                                        document["exception_source"] = "Shopify GraphQL Send Mutation";
                                        document["exception_stack_trace"] = "";
                                        document["seen"] = false;
                                        await exceptionCollection.InsertOneAsync(document);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            BsonDocument document = new BsonDocument();
                            document["service"] = "Shopify Update Price";
                            document["created_at"] = DateTime.Now;
                            document["product_id"] = id;
                            document["exception_message"] = ex.Message;
                            document["exception_source"] = ex.Source;
                            document["exception_stack_trace"] = ex.StackTrace;
                            document["seen"] = false;
                            await exceptionCollection.InsertOneAsync(document);
                        }
                    }

                    /*
                    foreach (var item in priceList)
                    {
                        try
                        {
                            var searchKeyValue = GlobalVariables.ShopifyConfig?.ItemSearchKey?.ToLower() == "alu" ? item.ALU : item.UPC;
                            var variantFilter = @$"{{""variantList.sku"":""{searchKeyValue}""}}";
                            var productResult = await shopifyProductCollection.Find(variantFilter).ToListAsync();
                            var productObject = productResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);

                            var product = JsonConvert.DeserializeObject<List<ProductsNode>>(JsonConvert.SerializeObject(productObject))?.FirstOrDefault() ?? null;
                            if (product != null)
                            {
                                var variant = product?.VariantList.FirstOrDefault(v => v.Sku == searchKeyValue) ?? null;

                                var productId = product?.Id;
                                var variantId = variant?.Id;

                                if (variant != null)
                                {
                                    var request = new GraphQLRequest
                                    {
                                        Query = @"
                                        mutation productVariantsBulkUpdate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
                                            productVariantsBulkUpdate(productId: $productId, variants: $variants) {
                                                productVariants {
                                                    id
                                                    price
                                                    compareAtPrice
                                                }
                                                userErrors {
                                                    field
                                                    message
                                                }
                                            }
                                        }",
                                        Variables = new
                                        {
                                            productId = productId,
                                            variants = new[]
                                            {
                                                new
                                                {
                                                    id = variantId,
                                                    price = item.Price,
                                                    compareAtPrice = item.CompareAtPrice,
                                                }
                                            }
                                        }
                                    };

                                    var response = await GraphAPI.SendMutation(request);
                                    if (response != null && response.Errors == null)
                                    {
                                        var filter = Builders<BsonDocument>.Filter.ElemMatch<BsonDocument>("variantList", Builders<BsonDocument>.Filter.Eq("sku", item.ALU));
                                        var projection = Builders<BsonDocument>.Projection.Include("variantList").Exclude("_id");
                                        var resultDoc = shopifyProductCollection.Find(filter).Project<BsonDocument>(projection).FirstOrDefault();
                                        if (resultDoc != null)
                                        {
                                            var variants = resultDoc["variantList"].AsBsonArray;

                                            var variantToUpdate = variants.FirstOrDefault(v => v["sku"] == item.ALU);
                                            if (variantToUpdate != null)
                                            {
                                                string inventoryId = variantToUpdate["id"].AsString;
                                                //decimal.TryParse(variantToUpdate["inventoryItem"]["unitCost"]["amount"].ToString(), out decimal cost);
                                                //if (cost != item?.Cost)
                                                //{}
                                                    List<ProductVariant> inventoryIds = new List<ProductVariant> { new ProductVariant() { Id = inventoryId, Sku=item?.ALU??"" } };
                                                    await ShopifyProduct.UpdateVariantsCost(inventoryIds, mongoDB);
                                            }
                                        }

                                        var updateFilter = Builders<BsonDocument>.Filter.Eq("alu", item?.ALU??"");

                                        var update = Builders<BsonDocument>.Update
                                             .Set("variantList.$[elem].price", item?.Price??0) // Updating price
                                             .Set("variantList.$[elem].compareAtPrice", item?.CompareAtPrice??0) // Updating compareAtPrice
                                             .Set("variantList.$[elem].inventoryItem.unitCost.amount", item?.Cost); // Updating unitCost.amount

                                        var arrayFilters = new List<ArrayFilterDefinition>
                                        {
                                            new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                                new BsonDocument("elem.inventoryItem.unitCost.amount", new BsonDocument("$exists", true))
                                            )
                                        };

                                        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

                                        var result1 = shopifyProductCollection.UpdateMany(filter, update, updateOptions);

                                        await itemPriceCollection.DeleteManyAsync(@$"{{{GlobalVariables.ShopifyConfig?.ItemSearchKey}:""{searchKeyValue}""}}");

                                        BsonDocument document = new BsonDocument();
                                        document["type"] = "Shopify Price Update";
                                        document["created_at"] = DateTime.Now;
                                        document["sku_alu"] = item?.ALU;
                                        document["price"] = item?.Price?.ToString("0.00");
                                        document["compareAtPrice"] = item?.CompareAtPrice?.ToString("0.00");
                                        await logCollection.InsertOneAsync(document);
                                    }
                                    else if (response?.Errors != null)
                                    {
                                        foreach (var err in response.Errors)
                                        {
                                            BsonDocument document = new BsonDocument();
                                            document["service"] = "Shopify Update Price";
                                            document["created_at"] = DateTime.Now;
                                            document["sku_alu"] = item?.ALU;
                                            document["exception_message"] = err.Message;
                                            document["exception_source"] = "Shopify GraphQL Send Mutation";
                                            document["exception_stack_trace"] = "";
                                            document["seen"] = false;
                                            await exceptionCollection.InsertOneAsync(document);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            BsonDocument document = new BsonDocument();
                            document["service"] = "Shopify Update Price";
                            document["created_at"] = DateTime.Now;
                            document["sku_alu"] = item?.ALU;
                            document["exception_message"] = ex.Message;
                            document["exception_source"] = ex.Source;
                            document["exception_stack_trace"] = ex.StackTrace;
                            document["seen"] = false;
                            await exceptionCollection.InsertOneAsync(document);
                        }
                    }

                    */
                }
                var timelaps = (DateTime.Now - startTime).TotalMinutes;

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
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
            var logCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
            var shopifyProductCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            var itemQuantityCollection = mongoDB.GetCollection<BsonDocument>("inv_qty_new");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");
            var inventory = mongoDB.GetCollection<BsonDocument>("inventory");
            try
            {
                //var pipeline = new[]
                //{
                //    // --- $lookup ---
                //    new BsonDocument("$lookup", new BsonDocument
                //    {
                //        { "from", "shopify_inventory" },
                //        { "localField", "alu" },
                //        { "foreignField", "variantList.sku" },
                //        { "as", "shopifyMatch" }
                //    }),

                //    // --- $unwind shopifyMatch ---
                //    new BsonDocument("$unwind", "$shopifyMatch"),

                //    // --- $unwind shopifyMatch.variantList ---
                //    new BsonDocument("$unwind", "$shopifyMatch.variantList"),

                //    // --- $match ---
                //    new BsonDocument("$match", new BsonDocument
                //    {
                //        { "$expr", new BsonDocument("$and", new BsonArray
                //            {
                //                new BsonDocument("$eq", new BsonArray { "$alu", "$shopifyMatch.variantList.sku" }),
                //                new BsonDocument("$ne", new BsonArray { "$store_oh", "$shopifyMatch.variantList.inventoryQuantity" })
                //            })
                //        }
                //    }),

                //    // --- $addFields ---
                //    new BsonDocument("$addFields", new BsonDocument
                //    {
                //        { "store_oh", new BsonDocument("$cond", new BsonDocument
                //            {
                //                { "if", new BsonDocument("$lt", new BsonArray { "$store_oh", 0 }) },
                //                { "then", 0 },
                //                { "else", "$store_oh" }
                //            })
                //        }
                //    }),

                //    // --- $project ---
                //    new BsonDocument("$project", new BsonDocument
                //    {
                //        { "_id", 0 },
                //        { "alu", 1 },
                //        { "Quantity", "$store_oh" }
                //    }),

                //    // --- $merge ---
                //    new BsonDocument("$merge", new BsonDocument
                //    {
                //        { "into", "inv_qty_new" },
                //        { "on", "alu" },
                //        { "whenMatched", "merge" },
                //        { "whenNotMatched", "insert" }
                //    })
                //};

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

                    // --- $unwind shopifyMatch.variantList.quantities ---
                    new BsonDocument("$unwind", "$shopifyMatch.variantList.quantities"),

                    // --- $match for 'on_hand' quantities ---
                    new BsonDocument("$match", new BsonDocument(
                        "shopifyMatch.variantList.quantities.name", "on_hand"
                    )),

                    // --- $match comparing store_oh vs on_hand quantity with ignore rule ---
                    new BsonDocument("$match", new BsonDocument
                    {
                        { "$expr", new BsonDocument("$and", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$alu", "$shopifyMatch.variantList.sku" }),
                                new BsonDocument("$ne", new BsonArray { "$store_oh", "$shopifyMatch.variantList.quantities.quantity" }),
                                new BsonDocument("$not", new BsonDocument("$and", new BsonArray
                                {
                                    new BsonDocument("$lt", new BsonArray { "$store_oh", 0 }),
                                    new BsonDocument("$eq", new BsonArray { "$shopifyMatch.variantList.quantities.quantity", 0 })
                                }))
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
                        { "Quantity", "$store_oh" },
                        { "shopify_on_hand", "$shopifyMatch.variantList.quantities.quantity" }
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

                var quantityListFilter = "{}";
                var quantityListResult = (await itemQuantityCollection.FindAsync(quantityListFilter)).ToList();
                var quantityListObject = quantityListResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var quantityList = JsonConvert.DeserializeObject<List<InventoryQuantity>>(JsonConvert.SerializeObject(quantityListObject))?.ToList() ?? [];

                foreach (var item in quantityList)
                {
                    var searchKeyValue = GlobalVariables.ShopifyConfig?.ItemSearchKey?.ToLower() == "alu" ? item.ALU : item.UPC;
                    try
                    {
                        var variantFilter = @$"{{""variantList.sku"":""{searchKeyValue}""}}";
                        var productResult = await shopifyProductCollection.Find(variantFilter).ToListAsync();
                        var productObject = productResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);

                        var product = JsonConvert.DeserializeObject<List<ProductsNode>>(JsonConvert.SerializeObject(productObject))?.FirstOrDefault() ?? null;

                        if (product != null)
                        {
                            var variant = product?.VariantList.FirstOrDefault(v => v.Sku == searchKeyValue) ?? null;
                            var inventoryItemId = variant?.InventoryId;
                            var location = variant?.Locations.Where(l => l.Name == GlobalVariables.ShopifyConfig?.ShopifyLocationName).FirstOrDefault();
                            var quantities = new List<object>();
                            quantities.Add(new
                            {
                                inventoryItemId = inventoryItemId,
                                locationId = location?.Id??"0",
                                quantity = item.Quantity < 0 ?0 : item.Quantity
                            });

                            /*
                              
                             * Code for for Multi Location on Shopify  // to first upadte retailpro code for multiple Locations
                            //var locations = variant.Locations;

                            //var quantities = new List<object>();

                            //foreach (var location in locations)
                            //{
                            //    var ohQty = item.Quantity;

                            //    quantities.Add(new
                            //    {
                            //        inventoryItemId = inventoryItemId,
                            //        locationId = location.Id,
                            //        quantity = ohQty
                            //    });
                            //}

                            */

                            var mutation = new GraphQLRequest
                            {
                                Query = @"
                                        mutation inventorySetOnHandQuantities($input: InventorySetOnHandQuantitiesInput!) {
                                            inventorySetOnHandQuantities(input: $input) {
                                                userErrors {
                                                    field
                                                    message
                                                }
                                                inventoryAdjustmentGroup {
                                                      createdAt
                                                      reason
                                                      changes {
                                                        name
                                                        delta
                                                      }
                                                }
                                            }
                                        }
                                    ",
                                Variables = new
                                {
                                    input = new
                                    {
                                        reason = "correction", // Reason for the adjustment
                                        setQuantities = quantities
                                    }
                                }
                            };

                            var qtyRes = await GraphAPI.SendMutation(mutation);

                            if (qtyRes != null && qtyRes.Errors == null)
                            {
                                await itemQuantityCollection.DeleteManyAsync(@$"{{alu:""{searchKeyValue}""}}");
                            }
                            else if (qtyRes != null && qtyRes.Errors != null)
                            {
                                string errorMessage = string.Join(", ", qtyRes.Errors.SelectMany(s => s.Message).ToList());
                                //throw new CustomException(message: errorMessage, styleSid: products.FirstOrDefault()?.STYLE_SID ?? "");

                                BsonDocument document = new BsonDocument();
                                document["service"] = "Shopify Update Quantity";
                                document["created_at"] = DateTime.Now;
                                document["sku_alu"] = searchKeyValue;
                                document["exception_message"] = errorMessage;
                                document["exception_source"] = "Update Quantity";
                                document["exception_stack_trace"] = "";
                                document["seen"] = false;
                                await exceptionCollection.InsertOneAsync(document);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        BsonDocument document = new BsonDocument();
                        document["service"] = "Shopify Update Quantity";
                        document["created_at"] = DateTime.Now;
                        document["sku_alu"] = searchKeyValue;
                        document["exception_message"] = ex.Message;
                        document["exception_source"] = ex.Source;
                        document["exception_stack_trace"] = ex.StackTrace;
                        document["seen"] = false;
                        await exceptionCollection.InsertOneAsync(document);
                    }
                }

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

        private async Task<bool> GetShopifyInventoryChanges()
        {
            DateTime lastFetchDateTime = DateTime.Now;
            //long totalInventory = 0;
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
            var itemChangeCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_change");
            var itemChangeSCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_change_s");
            var shopifyInventoryCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            var shopifyInventoryLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_log");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {
                bool hasNextRecord = true;
                string endCursor = string.Empty;
                int productCount = 200;

                var filter = "{type:'Shopify Inventory'}";
                var logResult = await shopifyInventoryLogCollection.Find(filter).ToListAsync();
                var logObject = logResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var log = JsonConvert.DeserializeObject<List<JObject>>(JsonConvert.SerializeObject(logObject))?.FirstOrDefault();
                DateTimeOffset LastUpdated = DateTimeOffset.Now;
                var startTime = DateTime.Now;
                if (log == null)
                {
                    BsonDocument document = new()
                    {
                        ["type"] = "Shopify Inventory",
                        ["created_at"] = DateTime.Now,
                        ["last_updated_at"] = DateTime.Now,
                        ["total_orders"] = 0,
                        ["processed"] = 0,
                        ["failed"] = 0
                    };
                    await shopifyInventoryLogCollection.InsertOneAsync(document);
                    LastUpdated = DateTime.Now;
                }
                else
                {
                    _ = DateTimeOffset.TryParse(log["last_updated_at"]?.ToString(), out DateTimeOffset LastUpdatedAt);

                    if (LastUpdatedAt != DateTimeOffset.MinValue)
                    {
                        LastUpdated = LastUpdatedAt;
                    }
                }

                await mongoDB.DropCollectionAsync("shopify_inventory_change_s");

                while (hasNextRecord)
                {
                    var updatedAt = LastUpdated.AddMinutes(-3).ToString("yyyy-MM-ddThh:mm:ssZ"); // .ToUniversalTime()
                    var param = @$"first: {productCount},query:""updated_at:>'{updatedAt}'""{(string.IsNullOrEmpty(endCursor) ? "" : $",after:\"{endCursor}\"")}";
                    var qry = GraphQuery.ProductInfo(param);
                    var invResponse = await GraphAPI.QueryAsync(qry);

                    if (invResponse != null)
                    {
                        var productdata = JsonConvert.DeserializeObject<ProductResponce>(JsonConvert.SerializeObject(invResponse.Data));
                        var productList = productdata?.Products.Edges.Select(p => p.Node).ToList() ?? [];

                        if (productList.Count > 0)
                        {
                            foreach (var product in productList)
                            {
                                _ = long.TryParse(product.Id.Split('/').Last(), out long ProductId);
                                product.ProductId = ProductId;
                                product.VariantList.AddRange(product?.Variants?.Edges.Select(v => v.Node) ?? []);
                                //product.Variants = null;

                                var skuListString = "'" + string.Join("' OR sku:'", product?.VariantList.Select(s => s.Sku).ToList() ?? []) + "'";
                                var invLevelQry = @$"
                                      query inventoryItems {{
                                          inventoryItems(first: 200, query: ""sku:{skuListString}"") {{
                                            edges {{
                                              node {{
                                                id
                                                tracked
                                                sku
                                                inventoryLevels(first:100)
                                                {{
                                                    edges{{
                                                        node{{
                                                            location
                                                            {{
                                                                id
                                                                name
                                                            }}
                                                        }}
                                                    }}
                                                }}
                                              }}
                                            }}
                                          }}
                                        }}
                                    ";

                                var invRes = (await GraphAPI.QueryAsync(invLevelQry))?.Data;
                                var locationsResponse = JsonConvert.DeserializeObject<InventoryItemModel>(JsonConvert.SerializeObject(invRes));

                                foreach (var item in product?.VariantList ?? [])
                                {
                                    var invLocations = locationsResponse?.InventoryItems.Edges.Select(s => s.Node).Where(s => s.Sku == item.Sku).ToList();
                                    var location = invLocations?.FirstOrDefault()?.InventoryLevels.Edges.Select(s => s.Node).Select(n => n.Location).ToList();
                                    item.InventoryId = invLocations?.FirstOrDefault()?.Id;
                                    item.Locations = location ?? [];
                                }

                                BsonDocument bsonDocument = BsonDocument.Parse(JsonConvert.SerializeObject(product, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                                await itemChangeSCollection.InsertOneAsync(bsonDocument);
                            }

                            hasNextRecord = productdata?.Products.PageInfo.HasNextPage ?? false;
                            endCursor = productdata?.Products.PageInfo.EndCursor ?? "";
                        }
                        else
                        {
                            hasNextRecord = false;
                        }
                    }
                }

                var NewItemPipeline = new[]
                {
                    new BsonDocument("$lookup",
                    new BsonDocument
                        {
                            { "from", "shopify_inventory_change" },
                            { "localField", "productId" },
                            { "foreignField", "productId" },
                            { "as", "original" }
                        }),
                    new BsonDocument("$project",
                    new BsonDocument
                        {
                            { "_id", 0 },
                            { "original._id", 0 }
                        }),
                    new BsonDocument("$unwind",
                    new BsonDocument
                        {
                            { "path", "$original" },
                            { "preserveNullAndEmptyArrays", true }
                        }),
                    new BsonDocument("$project",
                    new BsonDocument
                        {
                            { "current", "$$ROOT" },
                            { "original", "$original" }
                        }),
                    new BsonDocument("$match",
                    new BsonDocument("$expr",
                    new BsonDocument("$or",
                    new BsonArray
                                {
                                    new BsonDocument("$ne",
                                    new BsonArray
                                        {
                                            "$current.variantList",
                                            "$original.variantList"
                                        }),
                                    new BsonDocument("$ne",
                                    new BsonArray
                                        {
                                            "$current.status",
                                            "$original.status"
                                        })
                                }))),
                    new BsonDocument("$project",
                    new BsonDocument("current", 1))
                };

                var ChangeSet = await itemChangeSCollection.Aggregate<BsonDocument>(NewItemPipeline).ToListAsync();
                if (ChangeSet.Count > 0)
                {
                    await mongoDB.DropCollectionAsync("shopify_inventory_change");
                    await itemChangeCollection.InsertManyAsync(await itemChangeSCollection.Find("{}").ToListAsync());

                    var ChangeSetObject = ChangeSet.Select(s => s["current"]).ToList().ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var changeSetList = JsonConvert.DeserializeObject<List<ProductsNode>>(JsonConvert.SerializeObject(ChangeSetObject))?.ToList() ?? [];

                    foreach (var change in changeSetList)
                    {
                        var bsnChange = BsonDocument.Parse(JsonConvert.SerializeObject(change, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        var update = Builders<BsonDocument>.Update
                       .Set("status", bsnChange["status"])
                       .Set("updatedAt", bsnChange["updatedAt"]) // 2025-01-27T10:05:58  .ToString("yyyy-MM-ddTHH:mm:sszzz")
                       .Set("variantList", bsnChange["variantList"]);

                        await shopifyInventoryCollection.UpdateOneAsync($@"{{productId:{change.ProductId}}}", update);
                    }
                }
            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["service"] = "Fetch Shopify Inventory";
                document["created_at"] = DateTime.Now;
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;
                document["seen"] = false;
                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private async Task<bool> GetShopifyInventoryChanges_X()
        {
            DateTime lastFetchDateTime = DateTime.Now;
            string mongoConnectionString = MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(MongoDatabase);
            var itemChangeCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_change");
            var itemChangeSCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory_change_s");
            var shopifyInventoryCollection = mongoDB.GetCollection<BsonDocument>("shopify_inventory");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {
                // Ensure indexes for performance
                itemChangeCollection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("productId")));
                itemChangeSCollection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("productId")));
                shopifyInventoryCollection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("productId")));

                bool hasNextRecord = true;
                string endCursor = string.Empty;
                int productCount = 200;

                await mongoDB.DropCollectionAsync("shopify_inventory_change_s");

                while (hasNextRecord)
                {
                    // Fetch products in batches
                    var param = @$"first: {productCount},query:""updated_at:>2025-01-27T23:00:10Z""{(string.IsNullOrEmpty(endCursor) ? "" : $",after:\"{endCursor}\"")}";
                    var qry = GraphQuery.ProductInfo(param);
                    var invResponse = await GraphAPI.QueryAsync(qry);

                    if (invResponse == null) break;

                    var productdata = JsonConvert.DeserializeObject<ProductResponce>(JsonConvert.SerializeObject(invResponse.Data));
                    var productList = productdata?.Products.Edges.Select(p => p.Node).ToList();

                    if (productList?.Any() ?? false)
                    {
                        var tasks = productList.Select(async product =>
                        {
                            // Process product data
                            _ = long.TryParse(product.Id.Split('/').Last(), out long ProductId);
                            product.ProductId = ProductId;
                            product.VariantList.AddRange(product?.Variants?.Edges.Select(v => v.Node) ?? []);
                            //product.Variants = null;

                            var skuList = product?.VariantList.Select(s => s.Sku).ToList() ?? [];
                            var skuListString = string.Join(" OR ", skuList.Select(s => $"sku:'{s}'"));

                            // Fetch inventory levels
                            var invLevelQry = @$"
                                query inventoryItems {{
                                    inventoryItems(first: 200, query: ""{skuListString}"") {{
                                        edges {{
                                            node {{
                                                id
                                                tracked
                                                sku
                                                inventoryLevels(first:100) {{
                                                    edges {{
                                                        node {{
                                                            location {{
                                                                id
                                                                name
                                                            }}
                                                        }}
                                                    }}
                                                }}
                                            }}
                                        }}
                                    }}
                                }}";
                            var invRes = (await GraphAPI.QueryAsync(invLevelQry))?.Data;
                            var locationsResponse = JsonConvert.DeserializeObject<InventoryItemModel>(JsonConvert.SerializeObject(invRes));

                            foreach (var item in product?.VariantList ?? [])
                            {
                                var invLocations = locationsResponse?.InventoryItems.Edges.Select(s => s.Node).Where(s => s.Sku == item.Sku).ToList();
                                var location = invLocations?.FirstOrDefault()?.InventoryLevels.Edges.Select(s => s.Node).Select(n => n.Location).ToList();
                                item.InventoryId = invLocations?.FirstOrDefault()?.Id;
                                item.Locations = location ?? [];
                            }

                            BsonDocument bsonDocument = BsonDocument.Parse(JsonConvert.SerializeObject(product, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                            await itemChangeSCollection.InsertOneAsync(bsonDocument);
                        });

                        await Task.WhenAll(tasks);

                        hasNextRecord = productdata?.Products.PageInfo.HasNextPage ?? false;
                        endCursor = productdata?.Products.PageInfo.EndCursor ?? "";
                    }
                    else
                    {
                        hasNextRecord = false;
                    }
                }

                // Aggregate and update changes
                var newItemPipeline = new[]
                {
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "shopify_inventory_change" },
                        { "localField", "productId" },
                        { "foreignField", "productId" },
                        { "as", "matchedDocs" }
                    }),
                    new BsonDocument("$unwind", new BsonDocument
                    {
                        { "path", "$matchedDocs" },
                        { "preserveNullAndEmptyArrays", true }
                    }),
                    new BsonDocument("$addFields", new BsonDocument
                    {
                        { "isDifferent", new BsonDocument("$ne", new BsonArray
                            {
                                "$$ROOT",
                                "$matchedDocs"
                            })
                        }
                    }),
                    new BsonDocument("$match", new BsonDocument
                    {
                        { "$expr", new BsonDocument("$or", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$matchedDocs", BsonNull.Value }),
                                new BsonDocument("$eq", new BsonArray { "$isDifferent", true })
                            })
                        }
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "matchedDocs", 0 },
                        { "isDifferent", 0 }
                    })
                };

                var changeSet = await itemChangeSCollection.Aggregate<BsonDocument>(newItemPipeline).ToListAsync();

                // Bulk update inventory
                var updateTasks = changeSet.Select(async prod =>
                {
                    var update = Builders<BsonDocument>.Update
                        .Set("status", prod["status"])
                        .Set("updatedAt", prod["updatedAt"])
                        .Set("variantList", prod["variantList"]);

                    await shopifyInventoryCollection.UpdateOneAsync(
                        Builders<BsonDocument>.Filter.Eq("productId", prod["productId"]),
                        update,
                        new UpdateOptions { IsUpsert = true });
                });

                await Task.WhenAll(updateTasks);
            }
            catch (Exception ex)
            {
                // Log exceptions

                var document = new BsonDocument
                {
                    { "service", "Fetch Shopify Inventory" },
                    { "created_at", DateTime.Now },
                    { "exception_message", ex.Message },
                    { "exception_source", ex.Source },
                    { "exception_stack_trace", ex.StackTrace },
                    { "seen", false }
                };
                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private List<(string variantId, string price, string? compareAtPrice)> ConvertToVariantUpdates(List<ProductPrice> productPrices)
        {
            if (productPrices == null || !productPrices.Any())
            {
                throw new ArgumentException("Product price list is null or empty.");
            }

            // Verify all rows belong to the same product
            var productGIds = productPrices.Select(p => p.productGId).Distinct().ToList();
            if (productGIds.Count > 1)
            {
                throw new ArgumentException("All product prices must belong to the same product (same productGId).");
            }

            // Convert to tuple format
            var updates = productPrices
                .Where(p => !string.IsNullOrEmpty(p.variantGId) && p.Price.HasValue)
                .Select(p => (
                    variantId: p.variantGId!,
                    price: p.Price!.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    compareAtPrice: p.CompareAtPrice.HasValue
                        ? p.CompareAtPrice.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        : null
                ))
                .Take(100) // Shopify limits to 100 variants per mutation (unless Extended Variants is enabled)
                .ToList();

            if (!updates.Any())
            {
                throw new ArgumentException("No valid variant updates found (missing variantGId or Price).");
            }

            return updates;
        }

        public async Task<IGraphQLResponse> UpdateVariantPricesAsync(string productId, List<(string variantId, string price, string? compareAtPrice)> updates)
        {
            var input = new ProductVariantsBulkUpdateInput
            {
                ProductId = productId,
                Variants = updates.Select(u => new ProductVariantsBulkInput
                {
                    Id = u.variantId,
                    Price = u.price,
                    CompareAtPrice = u.compareAtPrice
                }).ToList()
            };

            var request = new GraphQLRequest
            {
                Query = @"
                mutation productVariantsBulkUpdate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
                  productVariantsBulkUpdate(productId: $productId, variants: $variants) {
                    product { id }
                    productVariants { id price compareAtPrice }
                    userErrors { field message }
                  }
                }",
                Variables = new { productId = input.ProductId, variants = input.Variants }
            };

            var response = await GraphAPI.SendMutation(request);

            return response;
        }
    }

    public class ProductVariantsBulkInput
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public string? CompareAtPrice { get; set; }
    }

    public class ProductVariantsBulkUpdateInput
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("variants")]
        public List<ProductVariantsBulkInput> Variants { get; set; } = new();
    }

    public class ProductVariantsBulkUpdateResponse
    {
        [JsonProperty("productVariantsBulkUpdate")]
        public BulkUpdatePayload Payload { get; set; }
    }

    public class BulkUpdatePayload
    {
        [JsonProperty("product")]
        public Product2 Product { get; set; }

        [JsonProperty("productVariants")]
        public List<ProductVariant2> ProductVariants { get; set; } = new();

        [JsonProperty("userErrors")]
        public List<UserError> UserErrors { get; set; } = new();
    }

    public class Product2
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ProductVariant2
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public string? CompareAtPrice { get; set; }
    }

    public class UserError
    {
        [JsonProperty("field")]
        public List<string> Field { get; set; } = new();

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}