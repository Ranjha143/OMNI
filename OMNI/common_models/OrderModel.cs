using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using OMNI.Shopify.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    public partial class OrderResponce
    {
        [JsonProperty("orders")]
        public Orders? Orders { get; set; }
    }

    public partial class Orders
    {
        [JsonProperty("edges")]
        public List<OrdersEdge>? Edges { get; set; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; set; }
    }

    public partial class OrdersEdge
    {
        [JsonProperty("node")]
        public OrderNode Node { get; set; }
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

        [JsonProperty("tags")]
        public List<String>? Tags { get; set; }

        [JsonProperty("events")]
        public List<EventNode> Events { get; set; } = [];

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

        [JsonProperty("currentTotalPriceSet")]
        public MoneyBag CurrentTotalPriceSet { get; set; }

        [JsonProperty("currentSubtotalPriceSet")]
        public MoneyBag CurrentSubtotalPriceSet { get; set; }

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

        [JsonProperty("isCancelled")]
        public bool IsCancelled { get; set; } = false;
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
        public decimal Amount { get; set; }

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

        [JsonProperty("isProcessed")]
        public Boolean IsProcessed { get; set; } = false;
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


    public partial class Events
    {
        [JsonProperty("edges")]
        public List<EventsEdge> Edges { get; set; }
    }

    public partial class EventsEdge
    {
        [JsonProperty("node")]
        public EventsNode Node { get; set; }
    }

    public partial class EventsNode
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("appTitle")]
        public string? AppTitle { get; set; }

        [JsonProperty("attributeToApp")]
        public bool AttributeToApp { get; set; }

        [JsonProperty("attributeToUser")]
        public bool AttributeToUser { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("criticalAlert")]
        public bool CriticalAlert { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
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
}