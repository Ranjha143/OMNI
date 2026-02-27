using GraphQL;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.GraphQL
{
    internal class ShopifyProduct
    {

        public static async Task<bool> CreateNew(List<ProductModel> products, IMongoDatabase mongoDB)
        {
            var inv_price_new = mongoDB.GetCollection<BsonDocument>("inv_price_new");

            try
            {
                var newProductQuery = @"
                mutation CreateProductWithOptions($input: ProductInput!) {
                    productCreate(input: $input) {
                        product {
                            id
                            title
                            options {
                                id
                                name
                                position
                                values
                            }
                            variants(first: 100) {
                                nodes {
                                    id
                                    title
                                    selectedOptions {
                                        name
                                        value
                                    }
                                }
                            }
                        }
                        userErrors {
                            field
                            message
                        }
                    }
                }";
                var colors = products.Where(s => s.ATTRIBUTE != null).Select(s => s.ATTRIBUTE).Distinct().ToList();
                var sizes = products.Where(s => s.ITEM_SIZE != null).Select(s => s.ITEM_SIZE).Distinct().ToList();

                Object? colorOption;
                if (colors.Count > 0)
                {
                    colorOption = new
                    {
                        name = "Color",
                        values = colors.Select(color => new { name = color }).ToList()
                    };
                }
                else
                {
                    colorOption = null;
                }

                Object? sizeOption;

                if (sizes.Count > 0)
                {
                    sizeOption = new
                    {
                        name = "Size",
                        values = sizes.Select(size => new { name = size }).ToList()
                    };
                }
                else
                {
                    sizeOption = null;
                }

                var productInput = new
                {
                    input = new
                    {
                        title = (products.FirstOrDefault()?.DESCRIPTION1 + " - " + products.FirstOrDefault()?.DESCRIPTION2).Trim(),
                        status = "DRAFT",
                        productOptions = new[] { colorOption, sizeOption },
                    }
                };
                var newProductRequest = new GraphQLRequest
                {
                    Query = newProductQuery,
                    Variables = productInput
                };
                var newProductResponse = await GraphAPI.SendMutation(newProductRequest);

                if (newProductResponse != null && newProductResponse.Errors == null)
                {
                    var firstVariant = products[0];

                    var productPricingResult = await inv_price_new.Find(@$"{{""{GlobalVariables.ShopifyConfig?.ItemSearchKey}"":""{firstVariant.ALU}""}}").ToListAsync();
                    var productPricingObject = productPricingResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                    var priceInfo = JsonConvert.DeserializeObject<List<ProductPrice>>(JsonConvert.SerializeObject(productPricingObject))?.FirstOrDefault();

                    var createdProdData = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(newProductResponse.Data ?? "{}"));
                    var productId = createdProdData?["productCreate"]?["product"]?["id"]?.ToString();
                    var variantId = createdProdData?["productCreate"]?["product"]?["variants"]?["nodes"]?[0]?["id"]?.ToString();

                    var firstVariantAddSkuQuery = @"
                    mutation UpdateVariant($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
                        productVariantsBulkUpdate( productId: $productId, variants: $variants) {
                            productVariants {
                                id
                                sku
                            }
                            userErrors {
                                field
                                message
                            }
                        }
                    }";

                    var updateInput = new
                    {
                        productId = productId,
                        variants = new[]
                        {
                            new
                            {
                                id = variantId,
                                inventoryItem = new
                                {
                                    sku = GlobalVariables.ShopifyConfig?.ItemSearchKey.ToLower() == "alu"?firstVariant.ALU:firstVariant.UPC,
                                    tracked = true
                                },
                                // cost = priceInfo?.Cost ?? 0,
                                price = priceInfo?.Price ?? 0,
                                compareAtPrice = priceInfo?.CompareAtPrice
                            }
                        }
                    };

                    var addVariantSku = new GraphQLRequest
                    {
                        Query = firstVariantAddSkuQuery,
                        Variables = updateInput
                    };

                    var skuAddedresponce = await GraphAPI.SendMutation(addVariantSku);
                    if (skuAddedresponce != null && skuAddedresponce.Errors != null)
                    {
                        // Write error for varinat sku addition failure
                    }

                    var firstVariantData = (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(skuAddedresponce?.Data)))?["productVariantsBulkUpdate"];
                    var firstVariantInfo = (JsonConvert.DeserializeObject<VariantCreateResponse>(JsonConvert.SerializeObject(firstVariantData)))?.ProductVariants.ToList()??[];

                    if (await UpdateVariantsCost(firstVariantInfo, mongoDB))
                    { }
                    else
                    { 
                       // todo log cost update error
                    }

                    var variantsQuery = @"
                    mutation CreateVariantsWithCost($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
                        productVariantsBulkCreate(productId: $productId, variants: $variants) {
                            productVariants {
                                id
                                title
                                sku
                                price
                                compareAtPrice
                                selectedOptions {
                                    name
                                    value
                                }
                            }
                            userErrors {
                                field
                                message
                            }
                        }
                    }";

                    var variants = new List<object>();
                    var balanceVariants = products.Where(p => p.ALU != firstVariant.ALU).ToList();

                    foreach (var item in balanceVariants)
                    {
                        productPricingResult = await inv_price_new.Find(@$"{{""{GlobalVariables.ShopifyConfig?.ItemSearchKey}"":""{item.ALU}""}}").ToListAsync();
                        productPricingObject = productPricingResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                        priceInfo = JsonConvert.DeserializeObject<List<ProductPrice>>(JsonConvert.SerializeObject(productPricingObject))?.FirstOrDefault();

                        variants.Add(
                            new
                            {
                                price = priceInfo?.Price,
                                compareAtPrice = priceInfo?.CompareAtPrice,
                                inventoryItem = new
                                {
                                    sku = GlobalVariables.ShopifyConfig?.ItemSearchKey.ToLower() == "alu" ? item.ALU : item.UPC
                                },
                                optionValues = new[]
                                {
                                    new { name = item.ATTRIBUTE, optionName = "Color" },
                                    new { name = item.ITEM_SIZE, optionName = "Size" }
                                }
                            }

                        );
                    }

                    var addVariantsRequest = new GraphQLRequest
                    {
                        Query = variantsQuery,
                        Variables = new
                        {
                            productId = productId,
                            variants = variants
                        }
                    };

                    var addVariantsResponse = await GraphAPI.SendMutation(addVariantsRequest);

                    if (addVariantsResponse != null && addVariantsResponse.Errors != null)
                    {
                        string errorMessage = string.Join(", ", addVariantsResponse.Errors.SelectMany(s => s.Message).ToList());
                        throw new CustomException(message: errorMessage, styleSid: products.FirstOrDefault()?.STYLE_SID ?? "");
                    }
                    else
                    {
                        var dataString = (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(addVariantsResponse?.Data)))["productVariantsBulkCreate"];
                        var variantsCreated = (JsonConvert.DeserializeObject<VariantCreateResponse>(JsonConvert.SerializeObject(dataString))).ProductVariants.ToList();

                        if (await UpdateVariantsCost(variantsCreated, mongoDB))
                        {
                            return true;
                        }
                        else
                        {
                            // todo log cost update error
                        }
                    }

                }
                else if (newProductResponse != null && newProductResponse.Errors != null)
                {
                    string errorMessage = string.Join(", ", newProductResponse.Errors.SelectMany(s => s.Message).ToList());
                    throw new CustomException(message: errorMessage, styleSid: products.FirstOrDefault()?.STYLE_SID ?? "");
                }
                else
                {
                    throw new CustomException(message: "no response from shopify GraphQL API ", styleSid: products.FirstOrDefault()?.STYLE_SID ?? "");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public static async Task<bool> AddVariants(string productId, List<ProductModel> products, IMongoDatabase mongoDB)
        {
            var inv_price_new = mongoDB.GetCollection<BsonDocument>("inv_price_new");

            try
            {
                var optionValueQuery = $@"
                            query GetProductOptions {{
                              product(id: ""{productId}"") {{
                                id
                                title
                                options {{
                                  id
                                  name
                                  position
                                  optionValues {{
                                    id
                                    name
                                    hasVariants
                                  }}
                                }}
                              }}
                            }}";

                var optionValueResult = (await GraphAPI.QueryAsync(optionValueQuery))?.Data;

                if (optionValueResult != null)
                {
                    var optionValueObject = JsonConvert.DeserializeObject<OptionValueInfoObject>(JsonConvert.SerializeObject(optionValueResult));
                    var existingOptions = optionValueObject.Product.Options;

                    if (existingOptions.Count() <= 1 && existingOptions.SelectMany(s => s.OptionValues).Count() <= 1 && existingOptions.SelectMany(s => s.OptionValues).FirstOrDefault().Name == "Default Title")
                    {
                        var colors = products.Where(s => s.ATTRIBUTE != null).Select(s => s.ATTRIBUTE).Distinct().ToList();
                        var sizes = products.Where(s => s.ITEM_SIZE != null).Select(s => s.ITEM_SIZE).Distinct().ToList();

                        var mutation = @"
                            mutation CreateProductOption($productId: ID!, $options: [OptionCreateInput!]!) {
                              productOptionsCreate(productId: $productId, options: $options) {
                                product {
                                  options {
                                    id
                                    name
                                    position
                                    optionValues {
                                      id
                                      name
                                      hasVariants
                                    }
                                  }
                                }
                                userErrors {
                                  field
                                  message
                                }
                              }
                            }";

                        var variables = new
                        {
                            productId = productId,
                            options = new[]
                                {
                                    new
                                    {
                                        name = "Color",
                                        values = colors.Select(c => new { name = c }).ToArray()
                                    },
                                    new
                                    {
                                        name = "Size",
                                        values = sizes.Select(s => new { name = s }).ToArray()
                                    }
                                }
                        };

                        var request = new GraphQLRequest
                        {
                            Query = mutation,
                            Variables = variables
                        };

                        var response = await GraphAPI.SendMutation(request);
                    }


                    var existVariantqry = $@"
                        query GetProductVariants {{
                            product(id: ""{productId}"") {{
                                id
                                title
                                variants(first: 100) {{
                                    edges {{
                                        node {{
                                            id
                                            inventoryItem{{ 
                                                sku 
                                            }}
                                            selectedOptions {{
                                                name
                                                value
                                            }}
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    ";

                    var existingVariantResult = (await GraphAPI.QueryAsync(existVariantqry))?.Data;

                    var existingVariantObject = JsonConvert.DeserializeObject<ExistingVariantResponce>(JsonConvert.SerializeObject(existingVariantResult));
                    var existionVariantOption = existingVariantObject.Product.Variants.Edges.Select(n => n.Node).ToList();

                    var variantsQuery = @"
                    mutation CreateOptionValuesForExistingProduct($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
                        productVariantsBulkCreate(productId: $productId, variants: $variants) {
                            productVariants {
                                id
                                title
                                sku
                                price
                                compareAtPrice
                                selectedOptions {
                                    name
                                    value
                                }
                            }
                            userErrors {
                                field
                                message
                            }
                        }
                    }";


                    var variants = new List<object>();
                    foreach (var item in products)
                    {
                        var productPricingResult = await inv_price_new.Find(@$"{{""{GlobalVariables.ShopifyConfig?.ItemSearchKey}"":""{item.ALU}""}}").ToListAsync();
                        var productPricingObject = productPricingResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                        var priceInfo = JsonConvert.DeserializeObject<List<ProductPrice>>(JsonConvert.SerializeObject(productPricingObject))?.FirstOrDefault();

                        var colorAndSizeExist = existionVariantOption.Where(s =>
                            s.SelectedOptions.Any(opt => opt.Name == "Color" && opt.Value == item.ATTRIBUTE) &&
                            s.SelectedOptions.Any(opt => opt.Name == "Size" && opt.Value == item.ITEM_SIZE)
                        ).FirstOrDefault();

                        if (colorAndSizeExist != null)
                        {
                            if (colorAndSizeExist.InventoryItem.Sku != item.ALU) // TODO get inc matching Key pair
                            {
                                //todo update variant replace SKU with ALU from retailpro
                            }
                        }
                        else if (colorAndSizeExist == null)
                        {
                            variants.Add(
                                new
                                {
                                   // cost = priceInfo?.Cost ?? 0,
                                    price = priceInfo?.Price,
                                    compareAtPrice = priceInfo?.CompareAtPrice,
                                    inventoryItem = new
                                    {
                                        sku = GlobalVariables.ShopifyConfig?.ItemSearchKey.ToLower() == "alu" ? item.ALU : item.UPC
                                    },
                                    optionValues = new[]
                                    {
                                        new { name = item.ATTRIBUTE, optionName = "Color" },
                                        new { name = item.ITEM_SIZE, optionName = "Size" }
                                    }
                                }
                            );
                        }


                    }

                    var addVariantsRequest = new GraphQLRequest
                    {
                        Query = variantsQuery,
                        Variables = new
                        {
                            productId = productId,
                            variants = variants
                        }
                    };
                    var addVariantsResponse = await GraphAPI.SendMutation(addVariantsRequest);
                    if (addVariantsResponse != null && addVariantsResponse.Errors == null)
                    {

                        var dataString = (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(addVariantsResponse?.Data)))["productVariantsBulkCreate"];
                        var variantsCreated = (JsonConvert.DeserializeObject<VariantCreateResponse>(JsonConvert.SerializeObject(dataString))).ProductVariants.ToList();

                        if (await UpdateVariantsCost(variantsCreated, mongoDB))
                        {
                            return true;
                        }
                        else
                        {
                            // todo log cost update error
                        }

                        // update Cost
                        return true;
                    }
                    else if (addVariantsResponse != null && addVariantsResponse.Errors != null)
                    {
                        string errorMessage = string.Join(", ", addVariantsResponse.Errors.SelectMany(s => s.Message).ToList());
                        throw new CustomException(message: errorMessage, styleSid: products.FirstOrDefault()?.STYLE_SID ?? "");
                    }
                    else
                    {
                        throw new CustomException(message: "no response from shopify GraphQL API ", styleSid: products.FirstOrDefault()?.STYLE_SID ?? "");
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

            return true;

        }




        public static async Task<bool> UpdateVariantsCost(List<ProductVariant> variantList, IMongoDatabase mongoDB)
        {
            var inv_price_new = mongoDB.GetCollection<BsonDocument>("inv_price_new");
            List<bool> updateResults = new List<bool>();
            foreach (var variant in variantList)
            {
                var productPricingResult = await inv_price_new.Find(@$"{{""{GlobalVariables.ShopifyConfig?.ItemSearchKey}"":""{variant.Sku}""}}").ToListAsync();
                var productPricingObject = productPricingResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var priceInfo = JsonConvert.DeserializeObject<List<ProductPrice>>(JsonConvert.SerializeObject(productPricingObject))?.FirstOrDefault();

                var inventoryItemId = await GetInventoryItemIds(variant.Id);
                var res = await UpdateInventoryCost(inventoryItemId,(priceInfo?.Cost ?? 0));
                updateResults.Add(res);
            }
            return !updateResults.Any(s=>s == false);
        }

        static async Task<string> GetInventoryItemIds(string variantId)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                query GetVariantInventoryItem($variantId: ID!) {
                  productVariant(id: $variantId) {
                    id
                    inventoryItem {
                      id
                    }
                  }
                }",
                Variables = new { variantId }
            };

            var response = await GraphAPI.QueryAsync(request);

            if (response.Errors != null && response.Errors.Length > 0)
            {
                Console.WriteLine("Error fetching inventory item IDs:");
                foreach (var error in response.Errors)
                    Console.WriteLine(error.Message);
                return null;
            }

            if (response.Data != null)
            {
                var data = (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(response?.Data)));
                var productVariantId = data["productVariant"]["id"].ToString();
                var inventoryItemId = data["productVariant"]["inventoryItem"]["id"].ToString();
                return inventoryItemId;
            }

            return null;
        }

        static async Task<bool> UpdateInventoryCost(string inventoryItemId, double cost)
        {
            var request = new GraphQLRequest
            {
                Query = @"
                mutation inventoryItemUpdate($id: ID!, $input: InventoryItemInput!) {
                  inventoryItemUpdate(id: $id, input: $input) {
                    inventoryItem {
                      id
                      unitCost {
                        amount
                        currencyCode
                      }
                    }
                    userErrors {
                      field
                      message
                    }
                  }
                }",
                Variables = new
                {
                    id = inventoryItemId,  // ✅ ID should be inside input
                    input = new  // ✅ Corrected field name from cost → unitCost
                    {
                        cost = cost,
                    }
                }
            };


            var respose = await GraphAPI.SendMutation(request);
            
            return respose?.Errors == null;
        }

    }



}
