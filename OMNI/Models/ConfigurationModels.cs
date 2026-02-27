using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PluginManager;
using RetailPro2_X;

namespace OMNI
{
    public class ConfigurationModels
    {
    }

    public class Configurations
    {
        //public string client_name { get; set; } = "";
        //public string platform1 { get; set; } = "RetailPro";
        //public string platform { get; set; } = "";
        //public string platform1 { get; set; } = "RetailPro";
        //public string platform2 { get; set; } = "Shopify";
        //public string platform_version { get; set; } = "";
        //public string db_user_name { get; set; } = "reportuser";
        //public string db_password { get; set; } = "report";
        //public string server_address { get; set; } = "";
        //public string db_server_port { get; set; } = "1521";
        //public string db_sid { get; set; } = "RPROODS";
        //public string platform2 { get; set; } = "Shopify";
        //public string store_identifier { get; set; } = "";
        //public string shopify_location_name { get; set; }
        //public string api_key { get; set; } = "";
        //public string api_access_token { get; set; } = "";
        //public bool inventory_service { get; set; }
        //public bool order_service { get; set; }

        public RetailProConfigurationInfo retailpro { get; set; }
        public ShopifyConfigurationInfo shopify { get; set; }
        public ServiceConfigurationInfo service { get; set; }
    }

    public partial class RetailProConfigurationInfo
    {
        [BsonId] // Marks this as the MongoDB document ID
        [BsonRepresentation(BsonType.ObjectId)] // Converts it from string to ObjectId automatically
        public string? _Id { get; set; }

        //public string client_name { get; set; } = "";
        //public string platform { get; set; } = "RetailPro";
        //public string platform_version { get; set; } = "";
        //public string db_user_name { get; set; } = "reportuser";
        //public string db_password { get; set; } = "report";
        //public string server_address { get; set; } = "";
        //public string db_server_port { get; set; } = "1521";
        //public string db_sid { get; set; } = "RPROODS";

        [JsonProperty("client_name")]
        public string ClientName { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("server_address")]
        public string ServerAddress { get; set; }

        //[JsonProperty("prism_port")]
        //public string PrismPortX { get; set; } = "9081";

        [JsonProperty("db_server_port")]
        public string DbPort { get; set; } = "1521";

        [JsonProperty("db_sid")]
        public string DbSid { get; set; }

        [JsonProperty("db_user_name")]
        public string DbUserName { get; set; }

        [JsonProperty("db_password")]
        public string DbPassword { get; set; }

        [JsonProperty("platform_version")]
        public string PlatformVersion { get; set; }

        [JsonProperty("sbs_no")]
        public string SBS_NO { get; set; }

        [JsonProperty("default_store_no")]
        public string DefaultStoreNo { get; set; }

        [JsonProperty("order_store_no")]
        public string OrderStoreNo { get; set; }

        [JsonProperty("store_name")]
        public string StoreName { get; set; }

        [JsonProperty("prism_user")]
        public string PrismUser { get; set; }

        [JsonProperty("prism_password")]
        public string PrismPassword { get; set; }

        [JsonProperty("workstation")]
        public string Workstation { get; set; }

        //[JsonProperty("orig_currency_name")]
        //public string OrigCurrencyName { get; set; }

        //[JsonProperty("currency_sid")]
        //public string CurrencySid { get; set; }

        //[JsonProperty("doc_fee_type_sid")]
        //public string DocFeeTypeSid { get; set; } = "725717528000119807";

        //[JsonProperty("enable_multistore_inventory")]
        //public bool EnableMultiStoreEnventory { get; set; }

        [JsonProperty("inventory_stores")]
        public string InventoryStores { get; set; }

        [JsonProperty("item_search_key")]
        public string ItemSearchkey { get; set; }

        [JsonProperty("price_levels")]
        public PriceLevel PriceLevels { get; set; }

        /*
          "sale_order_from":"New",
                    "sale_invoice_from":"Fulfilled",
         */

        //===================================================================================

        [JsonProperty("financial_status_for_so")]
        public string? FinancialStatusForSO { get; set; }

        [JsonProperty("fulfillment_status_for_so")]
        public string? FulfillmentStatusForSO { get; set; }

        [JsonProperty("financial_status_for_invoice")]
        public string? FinancialStatusForInvoice { get; set; }

        [JsonProperty("fulfillment_status_for_invoice")]
        public string? FulfillmentStatusForInvoice { get; set; }

        [JsonProperty("sale_fulfillment_direction")]
        public string SaleFulfillmentDirection { get; set; } = "NA";

        [JsonProperty("sale_refund_direction")]
        public string SaleRefundDirection { get; set; } = "NA";

        [JsonProperty("sale_cancellation_direction")]
        public string SaleCancellationDirection { get; set; } = "NA";

        [JsonProperty("TestMode")]
        public bool TestMode { get; set; } = false;

        //[JsonProperty("sale_refund_fetch")]
        //public bool SaleRefundFetch { get; set; }

        //===================================================================================

        //[JsonProperty("sale_order_from")]
        //public string? SaleOrderFrom { get; set; }

        //[JsonProperty("sale_invoice_from")]
        //public string? SaleInvoiceFrom { get; set; }

        //[JsonProperty("sale_order_fetch_state")]
        //public string? SaleOrderFetchState { get; set; }

        //[JsonProperty("fulfilled_order_as")]
        //public string FulfilledOrderAs { get; set; } = "NA";
    }

    //public class ShopifyConfigurationInfo
    //{
    //    [BsonId] // Marks this as the MongoDB document ID
    //    [BsonRepresentation(BsonType.ObjectId)] // Converts it from string to ObjectId automatically
    //    public string? _Id { get; set; }

    //    public string client_name { get; set; } = "";
    //    public string platform { get; set; } = "";
    //    public string platform_version { get; set; } = "";
    //    public string store_identifier { get; set; } = "";
    //    public string location_name { get; set; } = "";
    //    public string api_key { get; set; } = "";
    //    public string api_access_token { get; set; } = "";
    //    public string financial_status_for_so { get; set; } = "";
    //    public string fulfillment_status_for_so { get; set; } = "";
    //    public string financial_status_for_invoice { get; set; } = "";
    //    public string fulfillment_status_for_invoice { get; set; } = "";
    //    public string sale_order_fetch_state { get; set; } = "";
    //    public string sale_fulfillment_direction { get; set; } = "";
    //    public string sale_refund_direction { get; set; } = "";
    //    public string sale_cancellation_direction { get; set; } = "";
    //    public string timezone { get; set; } = "";
    //    public string timezoneOffset { get; set; } = "";
    //    public string item_search_key { get; set; } = "";

    //}

    //public class ServiceConfigurationInfo
    //{
    //    public string Client_name { get; set; } = "";
    //    public bool Inventory_service { get; set; }
    //    public bool Order_service { get; set; }

    //    [JsonProperty("service")]
    //    public string? Service { get; set; }

    //    [JsonProperty("interval")]
    //    public int Interval { get; set; }

    //    [JsonProperty("enabled")]
    //    public bool Enabled { get; set; }

    //}
}