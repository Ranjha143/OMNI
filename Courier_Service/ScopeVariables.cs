
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni_Courier_Service
{

    public partial class CourierResponse
    {
        [JsonProperty("AirwayBillNumber")]
        public string? AirwayBillNumber { get; set; }

        [JsonProperty("Code")]
        public long? Code { get; set; }

        [JsonProperty("Description")]
        public string? Description { get; set; }

        [JsonProperty("DestinationCode")]
        public string? DestinationCode { get; set; }
    }
    internal class ScopeVariables
    {
        public static bool RetailProInventoryWorker { get; set; } = false;
        
        public static string OracleConnectionString { get; set; } = string.Empty;
        public static string ShopifyAuthToken { get; set; }
        public static string BaseUrl { get; set; }
        public static string SiteUser { get; set; }
        public static string UserPassword { get; set; }
        public static bool InventoryServiceIsEnabled { get; set; }
        public static bool RetailProOrderWorker { get; set; } = false;
        public static bool RetailProRefundWorker { get; set; } = false;
        public static bool OrderServiceIsEnabled { get; set; }
        public static int InventoryServiceInterval { get; set; } = 5;
        public static int OrderServiceInterval { get; set; } = 5;
        public static RetailProConfigurationInfo RProConfig { get; set; }
        public static string MongoConnectionString { get; set; } = "mongodb://localhost:27017";
        public static string MongoDatabase { get; set; } = "logo";
        public static string? RetailProAuthSession { get; set; }


        public static bool C3X_isActive { get; set; } = false;
        public static bool iMile_isActive { get; set; } = false;
    }


    public partial class RetailProConfigurationInfo
    {
        [JsonProperty("client_name")]
        public string ClientName { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("server_address")]
        public string ServerAddress { get; set; }

        [JsonProperty("prism_port")]
        public string PrismPortX { get; set; } = "9081";

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
        public string SbsNo { get; set; }

        [JsonProperty("sbs_sid")]
        public string SbsSID { get; set; }

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

    public class PriceLevel
    {
        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("compareAtPrice")]
        public long CompareAtPrice { get; set; }

    }


    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }



    #region ===== Order Model


    public partial class OrderResponce
    {
        [JsonProperty("orders")]
        public Orders Orders { get; set; }
    }

    public partial class Orders
    {
        [JsonProperty("edges")]
        public List<OrdersEdge> Edges { get; set; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; set; }
    }

    public partial class OrdersEdge
    {
        [JsonProperty("node")]
        public OrderNode Node { get; set; }
    }

    public class StoreQuantities
    {
        [JsonProperty("sid")]
        public string SID { get; set; }

        [JsonProperty("alu")]
        public string ALU { get; set; }

        [JsonProperty("store_no")]
        public string STORE_NO { get; set; }

        [JsonProperty("store_name")]
        public string STORE_NAME { get; set; }

        [JsonProperty("qty")]
        public long QTY { get; set; }

    }

    public class StoreInfo
    {
        public string sid { get; set; }
        public string store_name { get; set; }
    }

    public partial class OrderNode
    {
        [BsonId] // Marks this as the MongoDB document ID
        [BsonRepresentation(BsonType.ObjectId)] // Converts it from string to ObjectId automatically
        public string? _Id { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("closedAt")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonProperty("cancelledAt")]
        public DateTimeOffset? CancelledAt { get; set; }

        [JsonProperty("cancelReason")]
        public string? CancelReason { get; set; }

        [JsonProperty("processedAt")]
        public DateTimeOffset? ProcessedAt { get; set; }

        [JsonProperty("fullyPaid")]
        public bool FullyPaid { get; set; }

        [JsonProperty("unpaid")]
        public bool Unpaid { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }


        [JsonProperty("displayFinancialStatus")]
        public string DisplayFinancialStatus { get; set; }

        [JsonProperty("displayFulfillmentStatus")]
        public string DisplayFulfillmentStatus { get; set; }

        [JsonProperty("edited")]
        public bool Edited { get; set; }

        [JsonProperty("shippingAddress")]
        public Address ShippingAddress { get; set; }

        [JsonProperty("billingAddress")]
        public Address BillingAddress { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("lineItems")]
        public LineItems? LineItems { get; set; }

        [JsonProperty("lineItemList")]
        public List<LineItemNode> LineItemList { get; set; } = [];

        [JsonProperty("fulfillmentsCount")]
        public FulfillmentsCount FulfillmentsCount { get; set; }

        [JsonProperty("fulfillments")]
        public List<Fulfillment> Fulfillments { get; set; }

        [JsonProperty("netPaymentSet")]
        public MoneyBag NetPaymentSet { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("originalTotalAdditionalFeesSet")]
        public object OriginalTotalAdditionalFeesSet { get; set; }

        [JsonProperty("originalTotalDutiesSet")]
        public object OriginalTotalDutiesSet { get; set; }

        [JsonProperty("originalTotalPriceSet")]
        public MoneyBag OriginalTotalPriceSet { get; set; }

        [JsonProperty("paymentGatewayNames")]
        public List<string> PaymentGatewayNames { get; set; }

        [JsonProperty("presentmentCurrencyCode")]
        public string PresentmentCurrencyCode { get; set; }

        [JsonProperty("refundable")]
        public bool Refundable { get; set; }

        [JsonProperty("requiresShipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("returnStatus")]
        public string ReturnStatus { get; set; }

        [JsonProperty("taxesIncluded")]
        public bool TaxesIncluded { get; set; }

        [JsonProperty("taxExempt")]
        public bool TaxExempt { get; set; }

        [JsonProperty("taxLines")]
        public List<TaxLine> TaxLines { get; set; }

        [JsonProperty("totalDiscountsSet")]
        public MoneyBag TotalDiscountsSet { get; set; }

        [JsonProperty("totalOutstandingSet")]
        public MoneyBag TotalOutstandingSet { get; set; }

        [JsonProperty("totalPriceSet")]
        public MoneyBag TotalPriceSet { get; set; }

        [JsonProperty("totalReceivedSet")]
        public MoneyBag TotalReceivedSet { get; set; }

        [JsonProperty("totalRefundedSet")]
        public MoneyBag TotalRefundedSet { get; set; }

        [JsonProperty("totalRefundedShippingSet")]
        public MoneyBag TotalRefundedShippingSet { get; set; }

        [JsonProperty("totalShippingPriceSet")]
        public MoneyBag TotalShippingPriceSet { get; set; }

        [JsonProperty("totalTaxSet")]
        public MoneyBag TotalTaxSet { get; set; }

        [JsonProperty("refunds")]
        public List<Refund> Refunds { get; set; }

        [JsonProperty("transactions")]
        public List<Transaction> Transactions { get; set; }




    }

    public partial class Address
    {
        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("company")]
        public object Company { get; set; }

        [JsonProperty("coordinatesValidated")]
        public bool CoordinatesValidated { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countryCodeV2")]
        public string CountryCodeV2 { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("formattedArea")]
        public string FormattedArea { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("latitude")]
        public decimal? Latitude { get; set; }

        [JsonProperty("longitude")]
        public decimal? Longitude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("provinceCode")]
        public string ProvinceCode { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("formatted", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Formatted { get; set; }
    }

    public partial class Customer
    {
        [JsonProperty("addresses")]
        public List<Address> Addresses { get; set; }

        [JsonProperty("amountSpent")]
        public AmountSpent AmountSpent { get; set; }

        [JsonProperty("canDelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("defaultAddress")]
        public Address DefaultAddress { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }

    public partial class AmountSpent
    {
        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public partial class Fulfillment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("deliveredAt")]
        public DateTimeOffset? DeliveredAt { get; set; }

        [JsonProperty("displayStatus")]
        public string DisplayStatus { get; set; }

        [JsonProperty("estimatedDeliveryAt")]
        public DateTimeOffset? EstimatedDeliveryAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("totalQuantity")]
        public long TotalQuantity { get; set; }

        [JsonProperty("trackingInfo")]
        public List<TrackingInfo> TrackingInfo { get; set; }

        [JsonProperty("fulfillmentLineItems")]
        public FulfillmentLineItems? FulfillmentLineItems { get; set; }

        [JsonProperty("fulfillmentLineItemList")]
        public List<FulfillmentLineItemNode> FulfillmentLineItemList { get; set; } = [];

    }

    public partial class FulfillmentLineItems
    {
        [JsonProperty("edges")]
        public List<FulfillmentLineItemsEdge> Edges { get; set; }
    }

    public partial class FulfillmentLineItemsEdge
    {
        [JsonProperty("node")]
        public FulfillmentLineItemNode Node { get; set; }
    }

    public partial class FulfillmentLineItemNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("discountedTotalSet")]
        public MoneyBag DiscountedTotalSet { get; set; }

        [JsonProperty("lineItem")]
        public LineItem LineItem { get; set; }

        [JsonProperty("originalTotalSet")]
        public MoneyBag OriginalTotalSet { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }
    }

    public partial class MoneyBag
    {
        [JsonProperty("presentmentMoney")]
        public AmountSpent PresentmentMoney { get; set; }

        [JsonProperty("shopMoney")]
        public AmountSpent ShopMoney { get; set; }
    }

    public partial class LineItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("currentQuantity")]
        public long CurrentQuantity { get; set; }

        [JsonProperty("discountedTotalSet")]
        public MoneyBag DiscountedTotalSet { get; set; }

        [JsonProperty("discountedUnitPriceAfterAllDiscountsSet")]
        public MoneyBag DiscountedUnitPriceAfterAllDiscountsSet { get; set; }

        [JsonProperty("discountedUnitPriceSet")]
        public MoneyBag DiscountedUnitPriceSet { get; set; }

        [JsonProperty("isGiftCard", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGiftCard { get; set; }

        [JsonProperty("unfulfilledQuantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnfulfilledQuantity { get; set; }

        [JsonProperty("originalTotalSet")]
        public MoneyBag OriginalTotalSet { get; set; }

        [JsonProperty("originalUnitPriceSet")]
        public MoneyBag OriginalUnitPriceSet { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("refundableQuantity")]
        public long RefundableQuantity { get; set; }

        [JsonProperty("requiresShipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("taxable")]
        public bool Taxable { get; set; }

        [JsonProperty("taxLines")]
        public List<TaxLine> TaxLines { get; set; }

        [JsonProperty("totalDiscountSet")]
        public MoneyBag TotalDiscountSet { get; set; }

        [JsonProperty("unfulfilledDiscountedTotalSet", NullValueHandling = NullValueHandling.Ignore)]
        public MoneyBag UnfulfilledDiscountedTotalSet { get; set; }

        [JsonProperty("unfulfilledOriginalTotalSet", NullValueHandling = NullValueHandling.Ignore)]
        public MoneyBag UnfulfilledOriginalTotalSet { get; set; }

        [JsonProperty("duties", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Duties { get; set; }

        [JsonProperty("nonFulfillableQuantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? NonFulfillableQuantity { get; set; }
    }

    public partial class TaxLine
    {
        [JsonProperty("priceSet")]
        public MoneyBag PriceSet { get; set; }

        [JsonProperty("rate")]
        public decimal? Rate { get; set; }

        [JsonProperty("ratePercentage")]
        public decimal? RatePercentage { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class FulfillmentsCount
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("precision")]
        public string Precision { get; set; }
    }

    public partial class LineItems
    {
        [JsonProperty("edges")]
        public List<LineItemsEdge> Edges { get; set; }
    }

    public partial class LineItemsEdge
    {
        [JsonProperty("node")]
        public LineItemNode Node { get; set; }
    }

    public partial class LineItemNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("currentQuantity")]
        public long CurrentQuantity { get; set; }

        [JsonProperty("refundableQuantity")]
        public long RefundableQuantity { get; set; }

        [JsonProperty("nonFulfillableQuantity")]
        public long NonFulfillableQuantity { get; set; }

        [JsonProperty("taxable")]
        public bool Taxable { get; set; }

        [JsonProperty("taxLines")]
        public List<TaxLine> TaxLines { get; set; }

        [JsonProperty("originalUnitPriceSet")]
        public MoneyBag OriginalUnitPriceSet { get; set; }

        [JsonProperty("discountedUnitPriceAfterAllDiscountsSet")]
        public MoneyBag DiscountedUnitPriceAfterAllDiscountsSet { get; set; }

        [JsonProperty("totalDiscountSet")]
        public MoneyBag TotalDiscountSet { get; set; }

        [JsonProperty("unfulfilledQuantity")]
        public long UnfulfilledQuantity { get; set; }

        [JsonProperty("requiresShipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("duties")]
        public List<object> Duties { get; set; }

        [JsonProperty("inv_quantities")]
        public List<StoreQuantities> InvQuantities { get; set; }

    }

    public partial class Refund
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("duties")]
        public List<object> Duties { get; set; }

        [JsonProperty("refundLineItems")]
        public RefundLineItems RefundLineItems { get; set; }

        [JsonProperty("transactions")]
        public Transactions? Transactions { get; set; }

        [JsonProperty("TransactionList")]
        public List<Transaction> TransactionList { get; set; } = [];

    }

    public partial class RefundLineItems
    {
        [JsonProperty("edges")]
        public List<RefundLineItemsEdge> Edges { get; set; }
    }

    public partial class RefundLineItemsEdge
    {
        [JsonProperty("node")]
        public RefundLineItemNode Node { get; set; }
    }

    public partial class RefundLineItemNode
    {
        [JsonProperty("lineItem")]
        public LineItem LineItem { get; set; }

        [JsonProperty("priceSet")]
        public MoneyBag PriceSet { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("subtotalSet")]
        public MoneyBag SubtotalSet { get; set; }

        [JsonProperty("totalTaxSet")]
        public MoneyBag TotalTaxSet { get; set; }
    }

    public partial class Transactions
    {
        [JsonProperty("edges")]
        public List<TransactionsEdge> Edges { get; set; }
    }

    public partial class TransactionsEdge
    {
        [JsonProperty("node")]
        public Transaction Node { get; set; }
    }

    public partial class Transaction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public string Amount { get; set; }

        [JsonProperty("amountSet")]
        public MoneyBag AmountSet { get; set; }

        [JsonProperty("fees")]
        public List<Fee> Fees { get; set; }

        [JsonProperty("formattedGateway")]
        public string FormattedGateway { get; set; }

        [JsonProperty("gateway")]
        public string Gateway { get; set; }

        [JsonProperty("processedAt")]
        public DateTimeOffset? ProcessedAt { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("authorizationCode")]
        public string AuthorizationCode { get; set; }

        [JsonProperty("settlementCurrency")]
        public string SettlementCurrency { get; set; }

        [JsonProperty("settlementCurrencyRate")]
        public decimal? SettlementCurrencyRate { get; set; }
    }

    public partial class Fee
    {
        [JsonProperty("amount")]
        public AmountSpent Amount { get; set; }
    }

    public partial class PageInfo
    {
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("endCursor")]
        public string EndCursor { get; set; }
    }

    public partial class TrackingInfo
    {
        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }



    public partial class SenderInfo
    {
        [JsonProperty("SendersAddress1")]
        public string SendersAddress1 { get; set; }

        [JsonProperty("SendersAddress2")]
        public string SendersAddress2 { get; set; }

        [JsonProperty("SendersCity")]
        public string SendersCity { get; set; }

        [JsonProperty("SendersPhone")]
        public string SendersPhone { get; set; }

        [JsonProperty("SenderEmail")]
        public string SenderEmail { get; set; }
    }

    #endregion


    public class OrderInfo : OrderNode
    {


        [JsonProperty("retailProSid")]
        public string RetailProSid { get; set; }
        public bool has_error { get; set; } = false;
        public string status { get; set; } = "";
        public string assigned_store_no { get; set; } = "";
        public string assigned_store_sid { get; set; } = "";
        public string assigned_store_name { get; set; } = "";
        public bool dispatched { get; set; } = false;
        public bool stock_transfered { get; set; } = false;

        [JsonProperty("courier")]
        public Courier Courier { get; set; }

        public string accepted_by_store { get; set; } = "pending";
        public string error_message { get; set; } = "";

        [JsonProperty("sender_info")]
        public SenderInfo SenderInfo { get; set; }
    }


    public partial class Courier
    {
        [JsonProperty("courier_name")]
        public string CourierName { get; set; } = "";

        [JsonProperty("cn_number")]
        public string CnNumber { get; set; } = "";

        [JsonProperty("destination_city")]
        public string DestinationCity { get; set; } = "";

        [JsonProperty("destination_address")]
        public string DestinationAddress { get; set; } = "";
    }



}
