using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public class ProductModel
    {
        [JsonProperty("sid")]
        public string SID { get; set; }

        [JsonProperty("sbs_no")]
        public int SBS_NO { get; set; }

        [JsonProperty("created_date")]
        public string CREATED_DATE { get; set; }

        [JsonProperty("style_sid")]
        public string STYLE_SID { get; set; }

        [JsonProperty("alu")]
        public string ALU { get; set; }

        [JsonProperty("sku")]
        public string SKU { get; set; }

        [JsonProperty("upc")]
        public string UPC { get; set; }

        [JsonProperty("description1")]
        public string DESCRIPTION1 { get; set; }

        [JsonProperty("description2")]
        public string DESCRIPTION2 { get; set; }

        [JsonProperty("description3")]
        public string DESCRIPTION3 { get; set; }

        [JsonProperty("description4")]
        public string DESCRIPTION4 { get; set; }

        [JsonProperty("attribute")]
        public string ATTRIBUTE { get; set; }

        [JsonProperty("item_size")]
        public string ITEM_SIZE { get; set; }

        [JsonProperty("long_description")]
        public string LONG_DESCRIPTION { get; set; }

        [JsonProperty("kit_type")]
        public int KIT_TYPE { get; set; }

        [JsonProperty("cost")]
        public decimal? COST { get; set; }

        [JsonProperty("inventory_id")]
        public string? InventoryId { get; set; }
    }

    public class ProductPrice
    {
        [JsonProperty("productId")]
        public string? productId { get; set; }

        [JsonProperty("productGId")]
        public string? productGId { get; set; }

        [JsonProperty("variantGId")]
        public string? variantGId { get; set; }

        [JsonProperty("alu")]
        public string? ALU { get; set; }

        [JsonProperty("upc")]
        public string? UPC { get; set; }

        [JsonProperty("cost")]
        public double? Cost { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public double? CompareAtPrice { get; set; }
    }

    internal class ShopifyProductModel
    {
    }

    public partial class ProductResponce
    {
        [JsonProperty("products")]
        public Products Products { get; set; }
    }

    public partial class Products
    {
        [JsonProperty("edges")]
        public List<ProductsEdge> Edges { get; set; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; set; }
    }

    public partial class ProductsEdge
    {
        [JsonProperty("node")]
        public ProductsNode Node { get; set; }
    }

    public partial class ProductsNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        public long ProductId { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }

        [JsonProperty("createdAt")]
        public string? CreatedAt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("descriptionHtml")]
        public string DescriptionHtml { get; set; }

        [JsonProperty("isGiftCard")]
        public bool? IsGiftCard { get; set; }

        [JsonProperty("priceRangeV2")]
        public PriceRangeV2 PriceRangeV2 { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("publishedAt")]
        public string? PublishedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("totalInventory")]
        public long? TotalInventory { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty("variantsCount")]
        public VariantsCount VariantsCount { get; set; }

        [JsonProperty("variants")]
        public Variants? Variants { get; set; }

        [JsonProperty("variantList")]
        public List<VariantsNode> VariantList { get; set; } = [];

        [JsonProperty("style_sid")]
        public string? StyleSid { get; set; }

        [JsonProperty("matched")]
        public bool Matched { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("isLeaf")]
        public bool IsLeaf { get; set; }

        [JsonProperty("isRoot")]
        public bool IsRoot { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        [JsonProperty("childrenIds")]
        public List<string> ChildrenIds { get; set; }
    }

    public partial class PriceRangeV2
    {
        [JsonProperty("maxVariantPrice")]
        public VariantPrice MaxVariantPrice { get; set; }

        [JsonProperty("minVariantPrice")]
        public VariantPrice MinVariantPrice { get; set; }
    }

    public partial class VariantPrice
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public partial class Variants
    {
        [JsonProperty("edges")]
        public List<VariantsEdge> Edges { get; set; }
    }

    public partial class VariantsEdge
    {
        [JsonProperty("node")]
        public VariantsNode Node { get; set; }
    }

    public partial class VariantsNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("availableForSale")]
        public bool AvailableForSale { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public double? CompareAtPrice { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("inventoryQuantity")]
        public long InventoryQuantity { get; set; }

        [JsonProperty("inventoryItem")]
        public InventoryItem InventoryItem { get; set; }

        [JsonProperty("inventory_id")]
        public string? InventoryId { get; set; }

        [JsonProperty("locations")]
        public List<Location> Locations { get; set; } = [];

        [JsonProperty("quantities")]
        public List<Quantity> Quantities { get; set; }

        // style_sid
    }

    public partial class InventoryItem
    {
        [JsonProperty("tracked")]
        public bool Tracked { get; set; }

        [JsonProperty("unitCost")]
        public MoneyBagV2 UnitCost { get; set; }

        [JsonProperty("inventoryLevels")]
        public InventoryLevels InventoryLevels { get; set; }
    }

    public partial class VariantsCount
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("precision")]
        public string Precision { get; set; }
    }

    public partial class PageInfo
    {
        //[JsonProperty("endCursor")]
        //public string EndCursor { get; set; }

        //[JsonProperty("hasNextPage")]
        //public bool HasNextPage { get; set; }
    }

    public class MoneyBagV2
    {
        [JsonProperty("amount")]
        public double? Amount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
    }
}