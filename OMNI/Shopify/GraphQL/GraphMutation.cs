using GraphQL;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using PluginManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Shopify
{
    internal static class GraphMutation
    {

        public static GraphQLRequest SetInventoryTrackStatusMutation(string inventoryItemId)
        {
            var mutation = new GraphQLRequest
            {
                Query = @"
                    mutation inventoryItemUpdate($input: InventoryItemInput!) {
                        inventoryItemUpdate(input: $input) {
                            inventoryItem {
                                id
                                tracked
                            }
                            userErrors {
                                field
                                message
                            }
                        }
                    }
                ",
                Variables = new
                {
                    input = new
                    {
                        id = inventoryItemId,
                        tracked = true
                    }
                }
            };
            return mutation;
        }

        public static GraphQLRequest AddVariant(string productId) //TODO: add Variant object
        {
            var mutation = new GraphQLRequest
            {
                Query = @"
                mutation productVariantCreate($input: ProductVariantInput!) {
                    productVariantCreate(input: $input) {
                        productVariant {
                            id
                            title
                            sku
                            price
                            compareAtPrice
                            inventoryItem {
                                id
                                tracked
                            }
                        }
                        userErrors {
                            field
                            message
                        }
                    }
                }
            ",
                Variables = new
                {
                    input = new
                    {
                        productId = productId,
                        title = "Black / XL",
                        sku = "SKU004",
                        price = "29.99",
                        compareAtPrice = "34.99",
                        options = new[] { "Black", "XL" }
                    }
                }
            };
            return mutation;
        }
    
    }
}
