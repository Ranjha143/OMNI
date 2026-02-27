using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailPro2_X.BL
{
    public partial class MagentoOrderModel
    {

        [JsonProperty("retail_pro_price_level")]
        public long? RetailProPriceLevel { get; set; }

        [JsonProperty("retail_pro_price_level_name")]
        public string RetailProPriceLevelName { get; set; }

        [JsonProperty("retail_pro_price_level_sid")]
        public string RetailProPriceLevelSid { get; set; }

        [JsonProperty("synced")]
        public bool Synced { get; set; }

        [JsonProperty("retailProSid")]
        public long? RetailProDocSid { get; set; }

        [JsonProperty("invoiced")]
        public bool Invoiced { get; set; }

        [JsonProperty("eefunded")]
        public bool Eefunded { get; set; }

        [JsonProperty("posting_retry")]
        public long? PostingRetry { get; set; }
        

        [JsonProperty("web_store")]
        public string WebStore { get; set; }



    }

    public partial class MagentoOrderModel
    {
        [JsonProperty("base_currency_code")]
        public string BaseCurrencyCode { get; set; }

        [JsonProperty("base_discount_amount")]
        public long? BaseDiscountAmount { get; set; }

        [JsonProperty("base_discount_invoiced")]
        public decimal? BaseDiscountInvoiced { get; set; }

        [JsonProperty("base_grand_total")]
        public decimal? BaseGrandTotal { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public decimal? BaseDiscountTaxCompensationAmount { get; set; }

        [JsonProperty("base_discount_tax_compensation_invoiced")]
        public decimal? BaseDiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("base_shipping_amount")]
        public decimal? BaseShippingAmount { get; set; }

        [JsonProperty("base_shipping_discount_amount")]
        public decimal? BaseShippingDiscountAmount { get; set; }

        [JsonProperty("base_shipping_discount_tax_compensation_amnt")]
        public decimal? BaseShippingDiscountTaxCompensationAmnt { get; set; }

        [JsonProperty("base_shipping_incl_tax")]
        public decimal? BaseShippingInclTax { get; set; }

        [JsonProperty("base_shipping_invoiced")]
        public decimal? BaseShippingInvoiced { get; set; }

        [JsonProperty("base_shipping_tax_amount")]
        public decimal? BaseShippingTaxAmount { get; set; }

        [JsonProperty("base_subtotal")]
        public decimal? BaseSubtotal { get; set; }

        [JsonProperty("base_subtotal_incl_tax")]
        public decimal? BaseSubtotalInclTax { get; set; }

        [JsonProperty("base_subtotal_invoiced")]
        public decimal? BaseSubtotalInvoiced { get; set; }

        [JsonProperty("base_tax_amount")]
        public decimal? BaseTaxAmount { get; set; }

        [JsonProperty("base_tax_invoiced")]
        public decimal? BaseTaxInvoiced { get; set; }

        [JsonProperty("base_total_due")]
        public decimal? BaseTotalDue { get; set; }

        [JsonProperty("base_total_invoiced")]
        public decimal? BaseTotalInvoiced { get; set; }

        [JsonProperty("base_total_invoiced_cost")]
        public decimal? BaseTotalInvoicedCost { get; set; }

        [JsonProperty("base_total_paid")]
        public decimal? BaseTotalPaid { get; set; }

        [JsonProperty("base_to_global_rate")]
        public decimal? BaseToGlobalRate { get; set; }

        [JsonProperty("base_to_order_rate")]
        public decimal? BaseToOrderRate { get; set; }

        [JsonProperty("billing_address_id")]
        public long? BillingAddressId { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("customer_email")]
        public string CustomerEmail { get; set; }

        [JsonProperty("customer_firstname")]
        public string CustomerFirstname { get; set; }

        [JsonProperty("customer_group_id")]
        public long? CustomerGroupId { get; set; }

        [JsonProperty("customer_is_guest")]
        public long? CustomerIsGuest { get; set; }

        [JsonProperty("customer_lastname")]
        public string CustomerLastname { get; set; }

        [JsonProperty("customer_note_notify")]
        public long? CustomerNoteNotify { get; set; }

        [JsonProperty("customer_prefix")]
        public string CustomerPrefix { get; set; }

        [JsonProperty("discount_amount")]
        public decimal? DiscountAmount { get; set; }

        [JsonProperty("discount_invoiced")]
        public decimal? DiscountInvoiced { get; set; }

        [JsonProperty("email_sent")]
        public long? EmailSent { get; set; }

        [JsonProperty("entity_id")]
        public long? EntityId { get; set; }

        [JsonProperty("global_currency_code")]
        public string GlobalCurrencyCode { get; set; }

        [JsonProperty("grand_total")]
        public long? GrandTotal { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public decimal? DiscountTaxCompensationAmount { get; set; }

        [JsonProperty("discount_tax_compensation_invoiced")]
        public decimal? DiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("increment_id")]
        public string IncrementId { get; set; }

        [JsonProperty("is_virtual")]
        public long? IsVirtual { get; set; }

        [JsonProperty("order_currency_code")]
        public string OrderCurrencyCode { get; set; }

        [JsonProperty("protect_code")]
        public string ProtectCode { get; set; }

        [JsonProperty("quote_id")]
        public long? QuoteId { get; set; }

        [JsonProperty("remote_ip")]
        public string RemoteIp { get; set; }

        [JsonProperty("shipping_amount")]
        public decimal? ShippingAmount { get; set; }

        [JsonProperty("shipping_description")]
        public string ShippingDescription { get; set; }

        [JsonProperty("shipping_discount_amount")]
        public decimal? ShippingDiscountAmount { get; set; }

        [JsonProperty("shipping_discount_tax_compensation_amount")]
        public decimal? ShippingDiscountTaxCompensationAmount { get; set; }

        [JsonProperty("shipping_incl_tax")]
        public decimal? ShippingInclTax { get; set; }

        [JsonProperty("shipping_invoiced")]
        public decimal? ShippingInvoiced { get; set; }

        [JsonProperty("shipping_tax_amount")]
        public decimal? ShippingTaxAmount { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("store_currency_code")]
        public string StoreCurrencyCode { get; set; }

        [JsonProperty("store_id")]
        public long? StoreId { get; set; }

        [JsonProperty("store_name")]
        public string StoreName { get; set; }

        [JsonProperty("store_to_base_rate")]
        public long? StoreToBaseRate { get; set; }

        [JsonProperty("store_to_order_rate")]
        public long? StoreToOrderRate { get; set; }

        [JsonProperty("subtotal")]
        public long? Subtotal { get; set; }

        [JsonProperty("subtotal_incl_tax")]
        public long? SubtotalInclTax { get; set; }

        [JsonProperty("subtotal_invoiced")]
        public long? SubtotalInvoiced { get; set; }

        [JsonProperty("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [JsonProperty("tax_invoiced")]
        public decimal? TaxInvoiced { get; set; }

        [JsonProperty("total_due")]
        public decimal? TotalDue { get; set; }

        [JsonProperty("total_invoiced")]
        public decimal? TotalInvoiced { get; set; }

        [JsonProperty("total_item_count")]
        public long? TotalItemCount { get; set; }

        [JsonProperty("total_paid")]
        public decimal? TotalPaid { get; set; }

        [JsonProperty("total_qty_ordered")]
        public long? TotalQtyOrdered { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("weight")]
        public long? Weight { get; set; }

        [JsonProperty("items")]
        public List<MagentoOrderModelItem> Items { get; set; }

        [JsonProperty("billing_address")]
        public Address BillingAddress { get; set; }

        [JsonProperty("payment")]
        public Payment Payment { get; set; }

        [JsonProperty("status_histories")]
        public List<StatusHistory> StatusHistories { get; set; }

        [JsonProperty("extension_attributes")]
        public MagentoOrderModelExtensionAttributes ExtensionAttributes { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("address_type")]
        public string AddressType { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country_id")]
        public string CountryId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("entity_id")]
        public long? EntityId { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("parent_id")]
        public long? ParentId { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("street")]
        public List<string> Street { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }
    }

    public partial class MagentoOrderModelExtensionAttributes
    {
        [JsonProperty("shipping_assignments")]
        public List<ShippingAssignment> ShippingAssignments { get; set; }

        [JsonProperty("payment_additional_info")]
        public List<PaymentAdditionalInfo> PaymentAdditionalInfo { get; set; }

        [JsonProperty("applied_taxes")]
        public List<object> AppliedTaxes { get; set; }

        [JsonProperty("item_applied_taxes")]
        public List<object> ItemAppliedTaxes { get; set; }

        [JsonProperty("am_giftcard_order")]
        public AmGiftcardOrder AmGiftcardOrder { get; set; }
    }

    public partial class AmGiftcardOrder
    {
        [JsonProperty("entity_id")]
        public long? EntityId { get; set; }

        [JsonProperty("order_id")]
        public long? OrderId { get; set; }

        [JsonProperty("gift_cards")]
        public List<object> GiftCards { get; set; }

        [JsonProperty("gift_amount")]
        public decimal? GiftAmount { get; set; }

        [JsonProperty("base_gift_amount")]
        public decimal? BaseGiftAmount { get; set; }

        [JsonProperty("invoice_gift_amount")]
        public decimal? InvoiceGiftAmount { get; set; }

        [JsonProperty("base_invoice_gift_amount")]
        public decimal? BaseInvoiceGiftAmount { get; set; }

        [JsonProperty("refund_gift_amount")]
        public decimal? RefundGiftAmount { get; set; }

        [JsonProperty("base_refund_gift_amount")]
        public decimal? BaseRefundGiftAmount { get; set; }

        [JsonProperty("applied_accounts")]
        public List<object> AppliedAccounts { get; set; }
    }

    public partial class PaymentAdditionalInfo
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class ShippingAssignment
    {
        [JsonProperty("shipping")]
        public Shipping Shipping { get; set; }

        [JsonProperty("items")]
        public List<ShippingAssignmentItem> Items { get; set; }
    }

    public partial class ShippingAssignmentItem
    {
        [JsonProperty("amount_refunded")]
        public decimal? AmountRefunded { get; set; }

        [JsonProperty("base_amount_refunded")]
        public decimal? BaseAmountRefunded { get; set; }

        [JsonProperty("base_discount_amount")]
        public decimal? BaseDiscountAmount { get; set; }

        [JsonProperty("base_discount_invoiced")]
        public decimal? BaseDiscountInvoiced { get; set; }

        [JsonProperty("base_discount_tax_compensation_invoiced")]
        public long? BaseDiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("base_original_price")]
        public decimal? BaseOriginalPrice { get; set; }

        [JsonProperty("base_price")]
        public decimal? BasePrice { get; set; }

        [JsonProperty("base_price_incl_tax")]
        public decimal? BasePriceInclTax { get; set; }

        [JsonProperty("base_row_invoiced")]
        public long? BaseRowInvoiced { get; set; }

        [JsonProperty("base_row_total")]
        public decimal? BaseRowTotal { get; set; }

        [JsonProperty("base_row_total_incl_tax")]
        public decimal? BaseRowTotalInclTax { get; set; }

        [JsonProperty("base_tax_amount")]
        public decimal? BaseTaxAmount { get; set; }

        [JsonProperty("base_tax_invoiced")]
        public decimal? BaseTaxInvoiced { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("discount_amount")]
        public decimal? DiscountAmount { get; set; }

        [JsonProperty("discount_invoiced")]
        public decimal? DiscountInvoiced { get; set; }

        [JsonProperty("discount_percent")]
        public decimal? DiscountPercent { get; set; }

        [JsonProperty("free_shipping")]
        public long? FreeShipping { get; set; }

        [JsonProperty("discount_tax_compensation_invoiced")]
        public decimal? DiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("is_qty_decimal?")]
        public long? IsQtydecimal { get; set; }

        [JsonProperty("is_virtual")]
        public long? IsVirtual { get; set; }

        [JsonProperty("item_id")]
        public long? ItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("no_discount")]
        public long? NoDiscount { get; set; }

        [JsonProperty("order_id")]
        public long? OrderId { get; set; }

        [JsonProperty("original_price")]
        public decimal? OriginalPrice { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("price_incl_tax")]
        public decimal? PriceInclTax { get; set; }

        [JsonProperty("product_id")]
        public long? ProductId { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("qty_canceled")]
        public long? QtyCanceled { get; set; }

        [JsonProperty("qty_invoiced")]
        public long? QtyInvoiced { get; set; }

        [JsonProperty("qty_ordered")]
        public long? QtyOrdered { get; set; }

        [JsonProperty("qty_refunded")]
        public long? QtyRefunded { get; set; }

        [JsonProperty("qty_shipped")]
        public long? QtyShipped { get; set; }

        [JsonProperty("quote_item_id")]
        public long? QuoteItemId { get; set; }

        [JsonProperty("row_invoiced")]
        public long? RowInvoiced { get; set; }

        [JsonProperty("row_total")]
        public double RowTotal { get; set; }

        [JsonProperty("row_total_incl_tax")]
        public long? RowTotalInclTax { get; set; }

        [JsonProperty("row_weight")]
        public long? RowWeight { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("store_id")]
        public long? StoreId { get; set; }

        [JsonProperty("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [JsonProperty("tax_invoiced")]
        public decimal? TaxInvoiced { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("weight")]
        public decimal? Weight { get; set; }

        [JsonProperty("product_option")]
        public PurpleProductOption ProductOption { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public decimal? BaseDiscountTaxCompensationAmount { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public decimal? DiscountTaxCompensationAmount { get; set; }

        [JsonProperty("parent_item_id")]
        public decimal? ParentItemId { get; set; }

        [JsonProperty("tax_percent")]
        public long? TaxPercent { get; set; }

        [JsonProperty("weee_tax_applied")]
        public string WeeeTaxApplied { get; set; }

        [JsonProperty("parent_item")]
        public ShippingAssignmentItem ParentItem { get; set; }
    }

    public partial class PurpleProductOption
    {
        [JsonProperty("extension_attributes")]
        public PurpleExtensionAttributes ExtensionAttributes { get; set; }
    }

    public partial class PurpleExtensionAttributes
    {
        [JsonProperty("bundle_options")]
        public List<BundleOption> BundleOptions { get; set; }

        [JsonProperty("configurable_item_options")]
        public List<ConfigurableItemOption> ConfigurableItemOptions { get; set; }
    }

    public partial class BundleOption
    {
        [JsonProperty("option_id")]
        public long? OptionId { get; set; }

        [JsonProperty("option_qty")]
        public long? OptionQty { get; set; }

        [JsonProperty("option_selections")]
        public List<long?> OptionSelections { get; set; }
    }

    public partial class ConfigurableItemOption
    {
        [JsonProperty("option_id")]
        public long? OptionId { get; set; }

        [JsonProperty("option_value")]
        public long? OptionValue { get; set; }
    }

    public partial class Shipping
    {
        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("total")]
        public Dictionary<string, decimal?> Total { get; set; }
    }

    public partial class MagentoOrderModelItem
    {
        [JsonProperty("amount_refunded")]
        public decimal? AmountRefunded { get; set; }

        [JsonProperty("base_amount_refunded")]
        public decimal? BaseAmountRefunded { get; set; }

        [JsonProperty("base_discount_amount")]
        public decimal? BaseDiscountAmount { get; set; }

        [JsonProperty("base_discount_invoiced")]
        public decimal? BaseDiscountInvoiced { get; set; }

        [JsonProperty("base_discount_tax_compensation_invoiced")]
        public decimal? BaseDiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("base_original_price")]
        public decimal? BaseOriginalPrice { get; set; }

        [JsonProperty("base_price")]
        public decimal? BasePrice { get; set; }

        [JsonProperty("base_price_incl_tax")]
        public decimal? BasePriceInclTax { get; set; }

        [JsonProperty("base_row_invoiced")]
        public decimal? BaseRowInvoiced { get; set; }

        [JsonProperty("base_row_total")]
        public decimal? BaseRowTotal { get; set; }

        [JsonProperty("base_row_total_incl_tax")]
        public decimal? BaseRowTotalInclTax { get; set; }

        [JsonProperty("base_tax_amount")]
        public decimal? BaseTaxAmount { get; set; }

        [JsonProperty("base_tax_invoiced")]
        public decimal? BaseTaxInvoiced { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("discount_amount")]
        public decimal? DiscountAmount { get; set; }

        [JsonProperty("discount_invoiced")]
        public decimal? DiscountInvoiced { get; set; }

        [JsonProperty("discount_percent")]
        public decimal? DiscountPercent { get; set; }

        [JsonProperty("free_shipping")]
        public long? FreeShipping { get; set; }

        [JsonProperty("discount_tax_compensation_invoiced")]
        public decimal? DiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("is_qty_decimal?")]
        public long? IsQtydecimal { get; set; }

        [JsonProperty("is_virtual")]
        public long? IsVirtual { get; set; }

        [JsonProperty("item_id")]
        public long? ItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("no_discount")]
        public long? NoDiscount { get; set; }

        [JsonProperty("order_id")]
        public long? OrderId { get; set; }

        [JsonProperty("original_price")]
        public decimal? OriginalPrice { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("price_incl_tax")]
        public decimal? PriceInclTax { get; set; }

        [JsonProperty("product_id")]
        public long? ProductId { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("qty_canceled")]
        public long? QtyCanceled { get; set; }

        [JsonProperty("qty_invoiced")]
        public long? QtyInvoiced { get; set; }

        [JsonProperty("qty_ordered")]
        public long? QtyOrdered { get; set; }

        [JsonProperty("qty_refunded")]
        public long? QtyRefunded { get; set; }

        [JsonProperty("qty_shipped")]
        public long? QtyShipped { get; set; }

        [JsonProperty("quote_item_id")]
        public long? QuoteItemId { get; set; }

        [JsonProperty("row_invoiced")]
        public decimal? RowInvoiced { get; set; }

        [JsonProperty("row_total")]
        public decimal? RowTotal { get; set; }

        [JsonProperty("row_total_incl_tax")]
        public decimal? RowTotalInclTax { get; set; }

        [JsonProperty("row_weight")]
        public long? RowWeight { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("store_id")]
        public long? StoreId { get; set; }

        [JsonProperty("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [JsonProperty("tax_invoiced")]
        public decimal? TaxInvoiced { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("weight")]
        public long? Weight { get; set; }

        [JsonProperty("product_option")]
        public FluffyProductOption ProductOption { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public long? BaseDiscountTaxCompensationAmount { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public long? DiscountTaxCompensationAmount { get; set; }

        [JsonProperty("parent_item_id")]
        public long? ParentItemId { get; set; }

        [JsonProperty("tax_percent")]
        public long? TaxPercent { get; set; }

        [JsonProperty("weee_tax_applied")]
        public string WeeeTaxApplied { get; set; }

        [JsonProperty("parent_item")]
        public MagentoOrderModelItem ParentItem { get; set; }
    }

    public partial class FluffyProductOption
    {
        [JsonProperty("extension_attributes")]
        public FluffyExtensionAttributes ExtensionAttributes { get; set; }
    }

    public partial class FluffyExtensionAttributes
    {
        [JsonProperty("bundle_options")]
        public List<BundleOption> BundleOptions { get; set; }
    }

    public partial class Payment
    {
        [JsonProperty("account_status")]
        public object AccountStatus { get; set; }

        [JsonProperty("additional_information")]
        public List<string> AdditionalInformation { get; set; }

        [JsonProperty("amount_authorized")]
        public decimal? AmountAuthorized { get; set; }

        [JsonProperty("amount_ordered")]
        public decimal? AmountOrdered { get; set; }

        [JsonProperty("amount_paid")]
        public decimal? AmountPaid { get; set; }

        [JsonProperty("base_amount_authorized")]
        public decimal? BaseAmountAuthorized { get; set; }

        [JsonProperty("base_amount_ordered")]
        public decimal? BaseAmountOrdered { get; set; }

        [JsonProperty("base_amount_paid")]
        public decimal? BaseAmountPaid { get; set; }

        [JsonProperty("base_shipping_amount")]
        public decimal? BaseShippingAmount { get; set; }

        [JsonProperty("base_shipping_captured")]
        public decimal? BaseShippingCaptured { get; set; }

        [JsonProperty("cc_exp_year")]
        public decimal? CcExpYear { get; set; }

        [JsonProperty("cc_last4")]
        public long? CcLast4 { get; set; }

        [JsonProperty("cc_ss_start_month")]
        public long? CcSsStartMonth { get; set; }

        [JsonProperty("cc_ss_start_year")]
        public long? CcSsStartYear { get; set; }

        [JsonProperty("cc_trans_id")]
        public string CcTransId { get; set; }

        [JsonProperty("cc_type")]
        public string CcType { get; set; }

        [JsonProperty("entity_id")]
        public long? EntityId { get; set; }

        [JsonProperty("last_trans_id")]
        public string LastTransId { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("parent_id")]
        public long? ParentId { get; set; }

        [JsonProperty("shipping_amount")]
        public decimal? ShippingAmount { get; set; }

        [JsonProperty("shipping_captured")]
        public decimal? ShippingCaptured { get; set; }
    }

    public partial class StatusHistory
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("entity_id")]
        public long? EntityId { get; set; }

        [JsonProperty("entity_name")]
        public string EntityName { get; set; }

        [JsonProperty("is_customer_notified")]
        public object IsCustomerNotified { get; set; }

        [JsonProperty("is_visible_on_front")]
        public long? IsVisibleOnFront { get; set; }

        [JsonProperty("parent_id")]
        public long? ParentId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class Address
    {

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("region_code")]
        public string RegionCode { get; set; }

        [JsonProperty("region_id")]
        public long? RegionId { get; set; }

        [JsonProperty("company")]
        public object Company { get; set; }
    }

    public partial class ExtensionAttributes
    {
        [JsonProperty("shipping_assignments")]
        public List<ShippingAssignment> ShippingAssignments { get; set; }

        [JsonProperty("payment_additional_info")]
        public List<PaymentAdditionalInfo> PaymentAdditionalInfo { get; set; }

        [JsonProperty("applied_taxes")]
        public List<object> AppliedTaxes { get; set; }

        [JsonProperty("item_applied_taxes")]
        public List<object> ItemAppliedTaxes { get; set; }

        [JsonProperty("am_giftcard_order")]
        public AmGiftcardOrder AmGiftcardOrder { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("amount_refunded")]
        public decimal? AmountRefunded { get; set; }

        [JsonProperty("base_amount_refunded")]
        public decimal? BaseAmountRefunded { get; set; }

        [JsonProperty("base_discount_amount")]
        public decimal? BaseDiscountAmount { get; set; }

        [JsonProperty("base_discount_invoiced")]
        public decimal? BaseDiscountInvoiced { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public decimal? BaseDiscountTaxCompensationAmount { get; set; }

        [JsonProperty("base_discount_tax_compensation_invoiced")]
        public decimal? BaseDiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("base_original_price")]
        public decimal? BaseOriginalPrice { get; set; }

        [JsonProperty("base_price")]
        public decimal? BasePrice { get; set; }

        [JsonProperty("base_price_incl_tax")]
        public decimal? BasePriceInclTax { get; set; }

        [JsonProperty("base_row_invoiced")]
        public decimal? BaseRowInvoiced { get; set; }

        [JsonProperty("base_row_total")]
        public decimal? BaseRowTotal { get; set; }

        [JsonProperty("base_row_total_incl_tax")]
        public decimal? BaseRowTotalInclTax { get; set; }

        [JsonProperty("base_tax_amount")]
        public decimal? BaseTaxAmount { get; set; }

        [JsonProperty("base_tax_invoiced")]
        public decimal? BaseTaxInvoiced { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("discount_amount")]
        public decimal? DiscountAmount { get; set; }

        [JsonProperty("discount_invoiced")]
        public decimal? DiscountInvoiced { get; set; }

        [JsonProperty("discount_percent")]
        public decimal? DiscountPercent { get; set; }

        [JsonProperty("free_shipping")]
        public decimal? FreeShipping { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public decimal? DiscountTaxCompensationAmount { get; set; }

        [JsonProperty("discount_tax_compensation_invoiced")]
        public decimal? DiscountTaxCompensationInvoiced { get; set; }

        [JsonProperty("is_qty_decimal?")]
        public object IsQtydecimal { get; set; }

        [JsonProperty("is_virtual")]
        public long? IsVirtual { get; set; }

        [JsonProperty("item_id")]
        public long? ItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("no_discount")]
        public long? NoDiscount { get; set; }

        [JsonProperty("order_id")]
        public long? OrderId { get; set; }

        [JsonProperty("original_price")]
        public decimal? OriginalPrice { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("price_incl_tax")]
        public decimal? PriceInclTax { get; set; }

        [JsonProperty("product_id")]
        public long? ProductId { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("qty_canceled")]
        public long? QtyCanceled { get; set; }

        [JsonProperty("qty_invoiced")]
        public long? QtyInvoiced { get; set; }

        [JsonProperty("qty_ordered")]
        public long? QtyOrdered { get; set; }

        [JsonProperty("qty_refunded")]
        public long? QtyRefunded { get; set; }

        [JsonProperty("qty_shipped")]
        public long? QtyShipped { get; set; }

        [JsonProperty("quote_item_id")]
        public long? QuoteItemId { get; set; }

        [JsonProperty("row_invoiced")]
        public long? RowInvoiced { get; set; }

        [JsonProperty("row_total")]
        public decimal? RowTotal { get; set; }

        [JsonProperty("row_total_incl_tax")]
        public decimal? RowTotalInclTax { get; set; }

        [JsonProperty("row_weight")]
        public long? RowWeight { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("store_id")]
        public long? StoreId { get; set; }

        [JsonProperty("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [JsonProperty("tax_invoiced")]
        public decimal? TaxInvoiced { get; set; }

        [JsonProperty("tax_percent")]
        public decimal? TaxPercent { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("weee_tax_applied")]
        public string WeeeTaxApplied { get; set; }
    }

    public partial class Item
    {
        //[JsonProperty("is_qty_decimal?")]
        //public long? IsQtydecimal { get; set; }

        [JsonProperty("weight")]
        public long? Weight { get; set; }

        [JsonProperty("product_option")]
        public ProductOption ProductOption { get; set; }

        [JsonProperty("parent_item_id")]
        public long? ParentItemId { get; set; }

        [JsonProperty("parent_item")]
        public Item ParentItem { get; set; }
    }

    public partial class ProductOption
    {
        [JsonProperty("extension_attributes")]
        public ProductOptionExtensionAttributes ExtensionAttributes { get; set; }
    }

    public partial class ProductOptionExtensionAttributes
    {
        [JsonProperty("bundle_options")]
        public List<BundleOption> BundleOptions { get; set; }
    }
}
