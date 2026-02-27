using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;
using Quartz;
using RestSharp;
using System.Globalization;

namespace RetailPro2_X
{

    public class PriceLevelInfo
    {

        //[JsonProperty("price_lvl_sid")]
        //public string price_lvl_sid { get; set; }

        //[JsonProperty("price_lvl_name")]
        //public string price_lvl_name { get; set; }

        //[JsonProperty("price_lvl")]
        //public Int32 price_lvl { get; set; }

        [JsonProperty("sid")]
        public string SID { get; set; }

        [JsonProperty("alu")]
        public string alu { get; set; }

        [JsonProperty("upc")]
        public string upc { get; set; }

        [JsonProperty("store_oh")]
        public decimal? store_oh { get; set; }

        [JsonProperty("cost")]
        public double? cost { get; set; }

        [JsonProperty("price")]
        public double? price { get; set; }

        [JsonProperty("compareAtPrice")]
        public double? compareAtPrice { get; set; }


    }

    internal class DefaultStoreOnHand
    {
        [JsonProperty("OH_QTY")]
        public long OhQty { get; set; }
    }
    internal class RetailPro
    {
    }
    public class StoreInfo
    {
        public string SID { get; set; }
        public string STORE_NAME { get; set; }
        public string SBS_SID { get; set; }
        public string STORE_CODE { get; set; }
        public string STORE_NO { get; set; }
        public string ACTIVE_PRICE_LVL_SID { get; set; }
    }

    public static class RetailProAuthentication
    {

        public static async Task<string?> GetSessionX(string user, string password)
        {
            string authSessionId = string.Empty;
            //string workStation = ConfigurationManager.AppSettings["Workstation"].ToString();
            //string serverIp = ConfigurationManager.AppSettings["ServerWebAddress"].ToString();
            try
            {
                var baseUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}";  // 

                var client = new RestClient(baseUrl + "/v1/rest/auth");

                var authNonceRequest = new RestRequest("", Method.Get);

                authNonceRequest.AddHeader("Accept", "application/Json,version=2.0");
                var authNonceResponse = await client.ExecuteAsync(authNonceRequest);

                var authNonce = Convert.ToDecimal(authNonceResponse.Headers.Where(w => w.Name != null && w.Name.Equals("Auth-Nonce"))
                    .Select(s => s.Value).FirstOrDefault());

                var authNonceValue = (Math.Truncate(authNonce / 13) % 99999) * 17;
                //=============================================================================================================> Acquire Auth-Session Token

                client = new RestClient(baseUrl + "/v1/rest/auth?usr=" + user + "&pwd=" + password);

                var authSessionRequest = new RestRequest("", Method.Get);
                authSessionRequest.AddHeader("Auth-Nonce", authNonce.ToString(CultureInfo.InvariantCulture));
                authSessionRequest.AddHeader("Auth-Nonce-Response", authNonceValue.ToString(CultureInfo.InvariantCulture));
                authSessionRequest.AddHeader("Accept", "application/Json,version=2.0");
                var authSessionResponse = await client.ExecuteAsync(authSessionRequest);

                if (authSessionResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    authSessionId = authSessionResponse.Headers.Where(w => w.Name != null && w.Name.Equals("Auth-Session"))
                        .Select(s => s.Value).FirstOrDefault()
                        ?.ToString();

                    client = new RestClient(baseUrl + "/v1/rest/sit?ws=" + GlobalVariables.RProConfig.Workstation);
                    var seatRequest = new RestRequest("", Method.Get);
                    seatRequest.AddHeader("Auth-Session", authSessionId ?? string.Empty);
                    seatRequest.AddHeader("Accept", "application/Json,version=2.0");
                    var authResponse = await client.ExecuteAsync(seatRequest);
                }
                //=============================================================================================================> Acquire Seat

                if (!string.IsNullOrEmpty(authSessionId))
                    return authSessionId;
                return "Error";
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        public static async Task<string?> GetSession(string user, string password, string workstation)
        {
            string authSessionId = string.Empty;
            try
            {
                var baseUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}"; // 
                var client = new RestClient($"{baseUrl}");

                var authSessionRequest = new RestRequest($"/api/security/login?appid=Prism-API-Explorer&pwd={password}&usr={user}&ws={workstation}", Method.Get);
                authSessionRequest.AddHeader("Accept", "application/Json,version=2.0");


                var authSessionResponse = await client.ExecuteAsync(authSessionRequest);
                var authInfo = JsonConvert.DeserializeObject<List<JObject>>(authSessionResponse?.Content??"[]")?.FirstOrDefault()?? [];


                authSessionId = authInfo["token"]?.ToString()??"";

                if (!string.IsNullOrEmpty(authSessionId))
                    return authSessionId;
                return null;
            }
            catch (Exception ex)
            {
                return null;

            }
        }



    }

    public class AcquireSeatInfo
    {
        public string AuthSession { get; set; }
    }


    public class APICall
    {
        APICall()
        {


        }

        public static async Task<RestResponse> GetExchangeRate(string targetLink)
        {
            try
            {
                var client = new RestClient(targetLink);
                ////client.Timeout = -1;
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Accept", "application/Json,version=2.0");
                RestResponse response = await client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static async Task<RestResponse> Get(string targetLink)
        {
            try
            {
                var client = new RestClient(targetLink);
                ////client.Timeout = -1;
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Accept", "application/Json,version=2.0");
                RestResponse response = await client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }


        #region Prism APIs

        public static async Task<RestResponse> GetAsync(string targetLink, string RetailProAuthSession)
        {
            try
            {
                var client = new RestClient(targetLink);
                ////client.Timeout = -1;
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Auth-Session", RetailProAuthSession);
                request.AddHeader("Accept", "application/Json,version=2.0");
                RestResponse response = await client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static async Task<RestResponse> Post(string targetLink, string requestBody, string SessionId)
        {
            try
            {
                var client = new RestClient(targetLink);
                //client.Timeout = -1;
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Auth-Session", SessionId);
                request.AddHeader("Accept", "application/Json,version=2.0");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(requestBody);
                // var xy = JsonConvert.SerializeObject(requestBody);
                RestResponse response = await client.ExecuteAsync(request);


                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }


        //public static RestResponse Post(string targetLink, object requestBody,string apiUser, string apiPassword)
        //{
        //    try
        //    {
        //        var client = new RestClient(targetLink);
        //        //client.Timeout = -1;
        //        var request = new RestRequest("",Method.Post);
        //        request.AddHeader("Auth", "user="+ apiUser + ",pass="+ apiPassword);
        //        request.AddHeader("Accept", "application/Json,version=2.0");
        //        request.RequestFormat = DataFormat.Json;
        //        request.AddJsonBody(requestBody);
        //        RestResponse response = client.ExecuteAsync(request).Result;

        //        return response;
        //    }
        //    catch (Exception e)
        //    {
        //        File.AppendAllText(@"D:\RPIntegrator\log\log.txt", "=======================================================" + Environment.NewLine);
        //        File.AppendAllText(@"D:\RPIntegrator\log\log.txt", "API POST: " + e.InnerException + "::" + e.Message + Environment.NewLine);
        //        File.AppendAllText(@"D:\RPIntegrator\log\log.txt", "=======================================================" + Environment.NewLine);
        //        return null;
        //    }
        //}

        public static async Task<RestResponse> PUT(string targetLink, string requestBody, string RPSessionID)
        {
            try
            {
                var client = new RestClient(targetLink);
                //client.Timeout = -1;
                var request = new RestRequest("", Method.Put);
                request.AddHeader("Auth-Session", RPSessionID);
                request.AddHeader("Accept", "application/Json,version=2.0");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(requestBody);
                RestResponse response = await client.ExecuteAsync(request);

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion
    }



    internal class RetailProModels
    {
    }

    public class CancleDocumentModel
    {
        public string SID { get; set; }
        public string ROW_VERSION { get; set; }
    }

    public partial class OrderCancleRequest
    {
        [JsonProperty("origin_application")]
        public string OriginApplication { get; set; }

        [JsonProperty("so_cancel_flag")]
        public string SoCancelFlag { get; set; }
    }

    public class Customer_Address_info
    {
        public string SID { get; set; }
        public string CUST_ID { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string PHONE_NO { get; set; }
        public string ADDRESS_1 { get; set; }
        public string ADDRESS_2 { get; set; }
        public string ADDRESS_3 { get; set; }
        public string CITY { get; set; }
        public string PRIMARY_FLAG { get; set; }
        public string ACTIVE { get; set; }
    }

    public class PostCustomer
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("store_sid")]
        public string store_sid { get; set; }

        [JsonProperty("last_name")]
        public string last_name { get; set; }

        [JsonProperty("first_name")]
        public string first_name { get; set; }

        [JsonProperty("customer_active")]
        public int customer_active { get; set; }

        [JsonProperty("customer_type")]
        public long customer_type { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("phones")]
        public List<Phone> phones { get; set; }

        [JsonProperty("address")]
        public List<Address> address { get; set; }
    }

    public partial class Phone
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("phone_no")]
        public string phone_no { get; set; }

        [JsonProperty("primary_flag")]
        public bool primary_flag { get; set; }

        [JsonProperty("seq_no")]
        public long seq_no { get; set; }
    }
    public partial class BillingAddress
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("customer_sid")]
        public string customer_sid { get; set; }

        //[JsonProperty("CUST_SID")]
        //public long CUST_SID { get; set; }

        [JsonProperty("primary_flag")]
        public bool primary_flag { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; }

        [JsonProperty("address_line_1")]
        public string address_line_1 { get; set; }

        [JsonProperty("address_line_2")]
        public string address_line_2 { get; set; }

        [JsonProperty("address_line_3")]
        public string address_line_3 { get; set; }

        //[JsonProperty("city")]
        //public string city { get; set; }

        [JsonProperty("address_line_4")]
        public object address_line_4 { get; set; }

        [JsonProperty("address_line_5")]
        public string address_line_5 { get; set; }

        [JsonProperty("address_line_6")]
        public object address_line_6 { get; set; }

        [JsonProperty("seq_no")]
        public long seq_no { get; set; }

        [JsonProperty("address_allow_contact")]
        public bool address_allow_contact { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; }

        [JsonProperty("customer_sid")]
        public string customer_sid { get; set; }

        [JsonProperty("primary_flag")]
        public bool primary_flag { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; }

        [JsonProperty("address_line_1")]
        public string address_line_1 { get; set; }

        [JsonProperty("address_line_2")]
        public string address_line_2 { get; set; }

        [JsonProperty("address_line_3")]
        public string address_line_3 { get; set; }

        //[JsonProperty("city")]
        //public string city { get; set; }

        [JsonProperty("address_line_4")]
        public object address_line_4 { get; set; }

        [JsonProperty("address_line_5")]
        public string address_line_5 { get; set; }

        [JsonProperty("address_line_6")]
        public object address_line_6 { get; set; }

        [JsonProperty("seq_no")]
        public long seq_no { get; set; }

        [JsonProperty("address_allow_contact")]
        public bool address_allow_contact { get; set; }

        [JsonProperty("CUST_SID")]
        public long CUST_SID { get; set; }
    }

    public class ItemInfo
    {
        public string SID { get; set; }
        public string UPC { get; set; }
        public string ALU { get; set; }
        public string DESCRIPTION1 { get; set; }
        public int ACTIVE { get; set; }
        public long STORE_OH { get; set; }
    }
    public class ItemPostInfo
    {
        // [{"manual_disc_value":3,"manual_disc_type":2}]

        public long? manual_disc_type { get; set; }
        public decimal? manual_disc_value { get; set; }
        public string manual_disc_reason { get; set; }
        public string origin_application { get; set; }
        public string ref_order_item_sid { get; set; }
        public string returned_item_invoice_sid { get; set; }
        public string invn_sbs_item_sid { get; set; }
        public int? order_type { get; set; }
        public int? item_type { get; set; }
        public long? quantity { get; set; }
        public long? qty_available_for_return { get; set; }
        public long? kit_type { get; set; }


        /*
         original_price: 695
original_tax_amount: 115.83
price: 695
tax_percent: 20
tax_amount: 115.83333
         */
        public decimal? original_price { get; set; }
        public decimal? price { get; set; }
        public long? price_lvl { get; set; }
        public decimal? tax_amount { get; set; }

        public string fulfill_store_sid { get; set; }
        public string return_reason { get; set; }

        public decimal? tax_percent { get; set; }


        public int? subloc_id { get; set; }
        public string document_sid { get; set; }
        public string central_document_sid { get; set; }
        public bool? detax_flag { get; set; }



    }

    public class PostTender
    {
        public string ORIGIN_APPLICATION { get; set; }
        public string TENDER_TYPE { get; set; }
        public string DOCUMENT_SID { get; set; }
        public string TAKEN { get; set; }
        public string TENDER_NAME { get; set; }
        public string AUTHORIZATION_CODE { get; set; }
    }

    public class PostDocument
    {
        public string ORIGIN_APPLICATION { get; set; }//"ORIGIN_APPLICATION":"RPROPRISMWEB",
        public int ORDER_STATUS { get; set; }//"ORDER_STATUS":0,
        public int ORDER_TYPE { get; set; }//"ORDER_TYPE":0,
        public long ORDER_QTY { get; set; }//"ORDER_QTY":10,
        public int ORDER_CHANGED_FLAG { get; set; }//"ORDER_CHANGED_FLAG":0,
        public int STATUS { get; set; }//"STATUS":0,
        public int IS_HELD { get; set; }//"IS_HELD":0,
        public int TENDER_TYPE { get; set; }//"TENDER_TYPE":-1,
        //public string STORE_SID { get; set; }
        public string STORE_UID { get; set; }
        public string STORE_NO { get; set; }// "STORE_NO":2,
        public string STORE_NAME { get; set; }
        public string CUSTOMER_PO_NUMBER { get; set; }//"CUST_PO_NO":"BL860",
        public string BT_CUID { get; set; }//"BT_CUID":"549239131000133175",
        public string BT_ID { get; set; }      //"BT_ID":100000340,
        public string BT_COUNTRY { get; set; }
        public string ST_CUID { get; set; }//"BT_CUID":"549239131000133175",
        public string ST_ID { get; set; }      //"BT_ID":100000340,
        public string ST_COUNTRY { get; set; }
        public long PRICE_LVL { get; set; }//"PRICE_LVL_NAME":"4",
        public long TOTAL_LINE_ITEM { get; set; }//"TOTAL_LINE_ITEM":"1",
        public long TOTAL_ITEM_COUNT { get; set; }//"TOTAL_ITEM_COUNT":"1",
        public int HAS_RETURN { get; set; }//"HAS_RETURN":"0",
        public int HAS_DEPOSIT { get; set; }//"HAS_DEPOSIT":"0"
        public string POS_FLAG1 { get; set; } //pos_flag3

        public string UDF_STRING1 { get; set; }

        public string SUBSIDIARY_UID { get; set; }
        //public string STORE_NUMBER { get; set; }

        public string SHOPIFY_ORDER_NO { get; set; }

        public string NOTES_GENERAL { get; set; }
        public string UDF_STRING2 { get; set; }
        public string UDF_STRING3 { get; set; }
        public string UDF_STRING4 { get; set; }
        public string UDF_STRING5 { get; set; }
        public string SHIP_DATE { get; set; }
        public string REF_ORDER_SID { get; set; }
        public string ref_sale_sid { get; set; }
        public decimal? TOTAL_DEPOSIT_USED { get; set; }
        //public string STORE_UID { get; set; }
        public string POS_FLAG3 { get; set; }
        public string POS_FLAG2 { get; set; }
        public string TRACKING_NUMBER { get; set; }
        public string BT_ADDRESS_LINE1 { get; set; }
        public string BT_ADDRESS_LINE2 { get; set; }
        public string BT_ADDRESS_LINE3 { get; set; }
        public string BT_ADDRESS_LINE4 { get; set; }
        public string BT_ADDRESS_LINE5 { get; set; }
        public string ST_ADDRESS_LINE1 { get; set; }
        public string ST_ADDRESS_LINE2 { get; set; }
        public string ST_ADDRESS_LINE3 { get; set; }
        public string ST_ADDRESS_LINE4 { get; set; }
        public string ST_ADDRESS_LINE5 { get; set; }
        public bool SEND_SALE_FULFILLMENT { get; set; }

        public bool SO_CANCEL_FLAG { get; set; } = false;
        public bool DETAX_FLAG { get; set; } = false;
        public decimal? ORDER_SHIPPING_AMT_MANUAL { get; set; }
        public decimal? shipping_amt_manual { get; set; }
        public decimal? shipping_amt_manual_returned { get; set; }

        public decimal? ORDER_FEE_AMT1 { get; set; }
        public string ORDER_FEE_TYPE1_SID { get; set; }

        public decimal? FEE_AMT1 { get; set; }
        public string FEE_TYPE1_SID { get; set; }
        public string UDF_DATE1 { get; set; }

    }

    public partial class PutShippingInfo
    {
        public string shipping_amt_manual { get; set; }
        //public string ORDER_SHIPPING_AMT_MANUAL { get; set; }
    }

    public partial class PutOrderDiscount
    {
        public string MANUAL_ORDER_DISC_TYPE { get; set; }
        public string MANUAL_ORDER_DISC_VALUE { get; set; }
        public string MANUAL_DISC_REASON { get; set; }
        public string MANUAL_ORDER_DISC_REASON { get; set; }
    }

    public class PutTaxInfo
    {
        public string TAX_AREA_NAME { get; set; }
    }

    public partial class PostResponseObject
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("row_version")]
        public long RowVersion { get; set; }

        [JsonProperty("transaction_total_tax_amt")]
        public decimal? TransactionTotalTaxAmt { get; set; }

        [JsonProperty("order_total_tax_amt")]
        public decimal? OrderTotalTaxAmt { get; set; }

        [JsonProperty("order_fee_amt1")]
        public decimal? OrderFeeAmt1 { get; set; }

    }

    public class OrderStatusPut
    {
        public int ORDER_TYPE { get; set; }
        public int STATUS { get; set; }
        //public string UDF_STRING5 { get; set; }
        //public decimal? UDF_FLOAT1 { get; set; }
    }

    public partial class TenderRequest1
    {
        [JsonProperty("tenders")]
        public List<object> tenders { get; set; }

        [JsonProperty("so_deposit_amt_paid")]
        public decimal? so_deposit_amt_paid { get; set; }
    }

    public partial class TenderRequest2
    {
        [JsonProperty("origin_application")]
        public string origin_application { get; set; } = "RProPrismWeb";

        [JsonProperty("tender_type")]
        public long tender_type { get; set; }

        [JsonProperty("document_sid")]
        public string document_sid { get; set; }

        [JsonProperty("taken")]
        public string taken { get; set; }

        [JsonProperty("given")]
        public string given { get; set; }

        [JsonProperty("tender_name")]
        public string tender_name { get; set; }

        [JsonProperty("card_type_name")]
        public string card_type_name { get; set; }

        [JsonProperty("authorization_code")]
        public string authorization_code { get; set; }
    }

    public partial class OrderObject
    {
        [JsonProperty("items")]
        public List<OrderInfo> OrderInfoList { get; set; }
    }

    public partial class OrderInfo
    {
        [JsonProperty("base_currency_code")]
        public string BaseCurrencyCode { get; set; }

        [JsonProperty("base_discount_amount")]
        public decimal? BaseDiscountAmount { get; set; }

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
        public long? BaseToOrderRate { get; set; }

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
        public List<ShippingAssignmentItem> Items { get; set; }

        [JsonProperty("billing_address")]
        public Address BillingAddress { get; set; }

        [JsonProperty("payment")]
        public Payment Payment { get; set; }

        [JsonProperty("status_histories")]
        public List<StatusHistory> StatusHistories { get; set; }

        [JsonProperty("extension_attributes")]
        public ExtensionAttributes ExtensionAttributes { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("address_type")]
        public string AddressType { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

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

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("region_code")]
        public string RegionCode { get; set; }

        [JsonProperty("region_id")]
        public long? RegionId { get; set; }

        [JsonProperty("street")]
        public List<string> Street { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }
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

        [JsonProperty("tax_percent")]
        public decimal? TaxPercent { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("weee_tax_applied")]
        public string WeeeTaxApplied { get; set; }
    }

    public partial class Shipping
    {
        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("total")]
        public Dictionary<string, long?> Total { get; set; }
    }

    public partial class Address
    {

        //[JsonProperty("city")]
        //public string city { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

    }

    public partial class Payment
    {
        [JsonProperty("account_status")]
        public dynamic AccountStatus { get; set; }

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
        public long? CcExpYear { get; set; }

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
        public long? IsCustomerNotified { get; set; }

        [JsonProperty("is_visible_on_front")]
        public long? IsVisibleOnFront { get; set; }

        [JsonProperty("parent_id")]
        public long? ParentId { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
    }


}
