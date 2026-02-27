using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify
{
    internal class Models
    {
    }

    public partial class ProductCountInfo
    {
        [JsonProperty("productsCount")]
        public ProductsCount ProductsCount { get; set; }
    }

    public partial class ProductsCount
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("precision")]
        public string Precision { get; set; }
    }

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

    //public static partial class GlobalVariables
    //{
    //    public static bool ShopifyOrderWorker { get; set; } = false;
    //    public static bool ShopifyRefundWorker { get; set; } = false;
    //    public static bool ShopifyInventoryWorker { get; set; } = false;
    //    public static bool InventoryServiceIsEnabled { get; set; } = false;
    //    public static int InventoryServiceInterval { get; set; } = 1;
    //    public static bool OrderServiceIsEnabled { get; set; } = false;
    //    public static int OrderServiceInterval { get; set; } = 1;
    //    public static string mongoConnectionString { get; set; } = "mongodb://localhost:27017";
    //    public static string MongoDatabase { get; set; } = "logo";
    //    public static ShopifyConfigurationInfo ShopifyConfig { get; set; } = new ShopifyConfigurationInfo();

    //    public static bool CourierServiceisAvailable { get; set; } = false;
    //}
   
    
    //public partial class ShopifyConfigurationInfo
    //{
    //    [JsonProperty("client_name")]
    //    public string? ClientName { get; set; }

    //    [JsonProperty("platform")]
    //    public string? Platform { get; set; }

    //    [JsonProperty("platform_version")]
    //    public string? PlatformVersion { get; set; }

    //    [JsonProperty("store_identifier")]
    //    public string? StoreIdentifier { get; set; }

    //    [JsonProperty("location_name")]
    //    public string? ShopifyLocationName { get; set; }

    //    //[JsonProperty("api_key")]
    //    //public string? ApiKey { get; set; }

    //    [JsonProperty("api_access_token")]
    //    public string? ApiAccessToken { get; set; }

    //    [JsonProperty("graph_url")]
    //    public string? GraphUrl { get; set; }

    //    //===================================================================================

    //    [JsonProperty("financial_status_for_so")]
    //    public string? FinancialStatusForSO { get; set; }

    //    [JsonProperty("fulfillment_status_for_so")]
    //    public string? FulfillmentStatusForSO { get; set; }

    //    [JsonProperty("financial_status_for_invoice")]
    //    public string? FinancialStatusForInvoice { get; set; }

    //    [JsonProperty("fulfillment_status_for_invoice")]
    //    public string? FulfillmentStatusForInvoice { get; set; }

    //    [JsonProperty("sale_fulfillment_direction")]
    //    public string SaleFulfillmentDirection { get; set; } = "NA";

    //    [JsonProperty("sale_refund_direction")]
    //    public string SaleRefundDirection { get; set; } = "NA";

    //    [JsonProperty("sale_cancellation_direction")]
    //    public string SaleCancellationDirection { get; set; } = "NA";

    //    //[JsonProperty("sale_refund_fetch")]
    //    //public bool SaleRefundFetch { get; set; }

    //    //===================================================================================

    //    //[JsonProperty("sale_order_from")]
    //    //public string? SaleOrderFrom { get; set; }
    //    //[JsonProperty("sale_invoice_from")]
    //    //public string? SaleInvoiceFrom { get; set; }
    //    //[JsonProperty("sale_order_fetch_state")]
    //    //public string? SaleOrderFetchState { get; set; }





    //    [JsonProperty("timezone")]
    //    public string? Timezone { get; set; }

    //    [JsonProperty("timezoneOffset")]
    //    public string? TimezoneOffset { get; set; }

    //    [JsonProperty("item_search_key")]
    //    public string? ItemSearchKey { get; set; }

    //}


    public partial class UnmatchedDocuments
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("variantList")]
        public List<UnmatchedVariantList> VariantList { get; set; }

        [JsonProperty("matched")]
        public bool Matched { get; set; }
    }

    public partial class UnmatchedVariantList
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }
    }


    public class CustomException : Exception
    {
        public int ErrorCode { get; }
        public string ALU { get; }
        public string OrderNo { get; }
        public string StyleSid { get; }
        public CustomException(string message, int errorCode = 0, string orderNo = "", string Alu = "", string styleSid = "") : base(message)
        {
            ErrorCode = errorCode;
            ALU = Alu;
            OrderNo = orderNo;
            StyleSid = styleSid;
        }
    }

    public class CityModel
    {
        [JsonProperty("name")]
        public string CityName { get; set; } //name
    }


    public class store_Sbs_SID
    {
        public string STORE_NO { get; set; }
        public string STORE_SID { get; set; }
        public string SBS_SID { get; set; }
    }

    public partial class RpcResult
    {
        [JsonProperty("methodname")]
        public string Methodname { get; set; }

        [JsonProperty("comments")]
        public object Comments { get; set; }

        [JsonProperty("introspection")]
        public bool Introspection { get; set; }

        [JsonProperty("params")]
        public Params Params { get; set; }
    }

    public partial class Params
    {
        [JsonProperty("result")]
        public long Result { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }


    public partial class RefItemInfo
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("invn_sbs_item_sid")]
        public string InvnSbsItemSid { get; set; }
    }

    public partial class RPC_Response
    {
        [JsonProperty("methodname")]
        public string Methodname { get; set; }

        [JsonProperty("comments")]
        public object Comments { get; set; }

        [JsonProperty("introspection")]
        public bool Introspection { get; set; }

        [JsonProperty("params")]
        public Params Params { get; set; }
    }

    public partial class Params
    {
        [JsonProperty("docsid")]
        public string Docsid { get; set; }
    }




    public partial class ExchangeRateInfo
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("base_code")]
        public string BaseCode { get; set; }

        [JsonProperty("conversion_rates")]
        public Dictionary<string, decimal?> ConversionRates { get; set; }
    }

    public partial class RecordCount
    {
        // [{"COUNT(*)":1.0}]

        public long COUNT { get; set; }
    }

}
