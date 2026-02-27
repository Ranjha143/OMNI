//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;

//namespace Shopify
//{

//    public partial class OptionValueInfoObject
//    {
//        [JsonProperty("product")]
//        public Product Product { get; set; }
//    }

//    public partial class Product
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("title")]
//        public string Title { get; set; }

//        [JsonProperty("options")]
//        public List<Option> Options { get; set; }
//    }

//    public partial class Option
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("name")]
//        public string Name { get; set; }

//        [JsonProperty("position")]
//        public long Position { get; set; }

//        [JsonProperty("optionValues")]
//        public List<OptionValue> OptionValues { get; set; }
//    }

//    public partial class OptionValue
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("name")]
//        public string Name { get; set; }

//        [JsonProperty("hasVariants")]
//        public bool HasVariants { get; set; }
//    }

//    public partial class ExistingVariantResponce
//    {
//        [JsonProperty("product")]
//        public ProductX Product { get; set; }
//    }

//    public partial class ProductX
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("title")]
//        public string Title { get; set; }

//        [JsonProperty("variants")]
//        public Variants Variants { get; set; }
//    }

//    public partial class Variants
//    {
//        [JsonProperty("edges")]
//        public List<Edge> Edges { get; set; }
//    }

//    public partial class Edge
//    {
//        [JsonProperty("node")]
//        public Node Node { get; set; }
//    }

//    public partial class Node
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("inventoryItem")]
//        public InventoryItem InventoryItem { get; set; }

//        [JsonProperty("selectedOptions")]
//        public List<SelectedOption> SelectedOptions { get; set; }
//    }
//    public partial class InventoryItem
//    {
//        [JsonProperty("sku")]
//        public string Sku { get; set; }
//    }

//    public partial class SelectedOption
//    {
//        [JsonProperty("name")]
//        public string Name { get; set; }

//        [JsonProperty("value")]
//        public string Value { get; set; }
//    }


//    //========================

//    public partial class VariantCreateResponse
//    {
//        [JsonProperty("productVariants")]
//        public List<ProductVariant> ProductVariants { get; set; }

//        [JsonProperty("userErrors")]
//        public List<object> UserErrors { get; set; }
//    }

//    public partial class ProductVariant
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }

//        [JsonProperty("title")]
//        public string Title { get; set; }

//        [JsonProperty("sku")]
//        public string Sku { get; set; }

//        [JsonProperty("price")]
//        public string Price { get; set; }

//        [JsonProperty("compareAtPrice")]
//        public string CompareAtPrice { get; set; }

//        [JsonProperty("selectedOptions")]
//        public List<SelectedOption> SelectedOptions { get; set; }
//    }

//    //public partial class SelectedOption
//    //{
//    //    [JsonProperty("name")]
//    //    public string Name { get; set; }

//    //    [JsonProperty("value")]
//    //    public string Value { get; set; }
//    //}
//}
