using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PluginManager
{

    public partial class VariantListElement
    {
        [JsonProperty("id")]
        public string Id { get; set; }

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
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("inventoryQuantity")]
        public long InventoryQuantity { get; set; }

        [JsonProperty("inventoryItem")]
        public InventoryItem InventoryItem { get; set; }

        [JsonProperty("inventory_id")]
        public string InventoryId { get; set; }

        [JsonProperty("locations")]
        public List<Location> Locations { get; set; }
    }

    //public partial class InventoryItem
    //{
    //    //[JsonProperty("tracked")]
    //    //public bool Tracked { get; set; }
    //}

    //public partial class Location
    //{
    //    //[JsonProperty("id")]
    //    //public string Id { get; set; }

    //    //[JsonProperty("name")]
    //    //public string Name { get; set; }
    //}

}
