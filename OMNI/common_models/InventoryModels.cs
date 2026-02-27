using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    internal class InventoryModels
    {
    }

    public class InventoryQuantity
    {
        [JsonProperty("upc")]
        public string UPC { get; set; }

        [JsonProperty("alu")]
        public string ALU { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

    }

    //public partial class LocationQuantity
    //{
    //    [JsonProperty("sku")]
    //    public string SKU { get; set; }

    //    [JsonProperty("quantaties")]
    //    public List<Quantaty> Quantaties { get; set; }
    //}

    //public partial class Quantaty
    //{
    //    [JsonProperty("location")]
    //    public string Location { get; set; }

    //    [JsonProperty("quantity")]
    //    public long Quantity { get; set; }
    //}


    //================================================================

    public partial class InventoryItemModel
    {
        [JsonProperty("inventoryItems")]
        public InventoryItems InventoryItems { get; set; }
    }

    public partial class InventoryItems
    {
        [JsonProperty("edges")]
        public List<InventoryItemsEdge> Edges { get; set; }
    }

    public partial class InventoryItemsEdge
    {
        [JsonProperty("node")]
        public PurpleNode Node { get; set; }
    }

    public partial class PurpleNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("inventoryLevels")]
        public InventoryLevels InventoryLevels { get; set; }
    }

    public partial class InventoryLevels
    {
        [JsonProperty("edges")]
        public List<InventoryLevelsEdge> Edges { get; set; }
    }

    public partial class InventoryLevelsEdge
    {
        [JsonProperty("node")]
        public InventoryLevelNode Node { get; set; }
    }

    public partial class InventoryLevelNode
    {
        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("quantities")]
        public List<Quantity> Quantities { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Quantity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public long QuantityQuantity { get; set; }
    }
    //public partial class InventoryLocationResponce
    //{
    //    [JsonProperty("productVariants")]
    //    public ProductVariants ProductVariants { get; set; }
    //}

    //public partial class ProductVariants
    //{
    //    [JsonProperty("edges")]
    //    public List<ProductVariantsEdge> Edges { get; set; }
    //}

    //public partial class ProductVariantsEdge
    //{
    //    [JsonProperty("node")]
    //    public ProductVariantNode Node { get; set; }
    //}

    //public partial class ProductVariantNode
    //{
    //    [JsonProperty("id")]
    //    public string Id { get; set; }

    //    [JsonProperty("sku")]
    //    public string Sku { get; set; }

    //    [JsonProperty("inventoryItem")]
    //    public InventoryItem InventoryItem { get; set; }
    //}

    //public partial class InventoryItem
    //{
    //    [JsonProperty("id")]
    //    public string Id { get; set; }

    //    [JsonProperty("inventoryLevels")]
    //    public InventoryLevels InventoryLevels { get; set; }
    //}

    //public partial class InventoryLevels
    //{
    //    [JsonProperty("edges")]
    //    public List<InventoryLevelsEdge> Edges { get; set; }
    //}

    //public partial class InventoryLevelsEdge
    //{
    //    [JsonProperty("node")]
    //    public InventoryLevelNode Node { get; set; }
    //}

    //public partial class InventoryLevelNode
    //{
    //    [JsonProperty("location")]
    //    public Location Location { get; set; }
    //}

    //public partial class Location
    //{
    //    [JsonProperty("id")]
    //    public string Id { get; set; }

    //    [JsonProperty("name")]
    //    public string Name { get; set; }
    //}

}
