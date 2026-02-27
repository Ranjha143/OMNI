using Newtonsoft.Json;
using OMNI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager
{
    internal class Models
    {
    }

    public partial class ServiceConfigurationInfo
    {
        public string Client_name { get; set; } = "";
        public bool Inventory_service { get; set; }
        public bool Order_service { get; set; }

        [JsonProperty("service")]
        public string? Service { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }


    }

    public static partial class GlobalVariables
    {
        public static bool ShopifyOrderWorker { get; set; } = false;
        public static bool ShopifyFulfillOrderWorker { get; set; } = false;
        public static bool ShopifyRefundWorker { get; set; } = false;
        public static bool ShopifyInventoryWorker { get; set; } = false;
        public static bool InventoryServiceIsEnabled { get; set; } = false;
        public static int InventoryServiceInterval { get; set; } = 1;
        public static bool OrderServiceIsEnabled { get; set; } = false;
        public static int OrderServiceInterval { get; set; } = 1;
        public static string MongoConnectionString { get; set; } = "mongodb://localhost:27017"; // Default connection string
        public static string MongoDatabase { get; set; } = "logo"; // Default database name
        public static ShopifyConfigurationInfo ShopifyConfig { get; set; } = new ShopifyConfigurationInfo();

        public static bool CourierServiceisAvailable { get; set; } = false;



        public static string ShopifyAuthToken { get; set; }
        public static string BaseUrl { get; set; }
        public static string SiteUser { get; set; }
        public static string UserPassword { get; set; }
        public static string OracleConnectionString { get; set; }
        //public static bool InventoryServiceIsEnabled { get; set; }
        public static bool RetailProInventoryWorker { get; set; } = false;
        public static bool RetailProOrderWorker { get; set; } = false;
        public static bool RetailProRefundWorker { get; set; } = false;
        //public static bool OrderServiceIsEnabled { get; set; }
        //public static int InventoryServiceInterval { get; set; } = 5;
        //public static int OrderServiceInterval { get; set; } = 5;

        public static RetailProConfigurationInfo RProConfig { get; set; }
        //public static string MongoConnectionString { get; set; } = "mongodb://localhost:27017";
        //public static string MongoDatabase { get; set; } = "logo";
        public static string? RetailProAuthSession { get; set; }

    }

    public partial class ShopifyConfigurationInfo
    {
        [JsonProperty("client_name")]
        public string? ClientName { get; set; }

        [JsonProperty("platform")]
        public string? Platform { get; set; }

        [JsonProperty("platform_version")]
        public string? PlatformVersion { get; set; }

        [JsonProperty("store_identifier")]
        public string? StoreIdentifier { get; set; }

        [JsonProperty("location_name")]
        public string? ShopifyLocationName { get; set; }

        //[JsonProperty("api_key")]
        //public string? ApiKey { get; set; }

        [JsonProperty("api_access_token")]
        public string? ApiAccessToken { get; set; }

        [JsonProperty("graph_url")]
        public string? GraphUrl { get; set; }

        [JsonProperty("sale_order_fetch_state")]
        public string? SaleOrderFetchState { get; set; }

        [JsonProperty("sale_fulfillment_direction")]
        public string SaleFulfillmentDirection { get; set; } = "NA";

        [JsonProperty("sale_refund_direction")]
        public string SaleRefundDirection { get; set; } = "NA";

        [JsonProperty("sale_cancellation_direction")]
        public string SaleCancellationDirection { get; set; } = "NA";

        [JsonProperty("sale_refund_fetch")]
        public bool SaleRefundFetch { get; set; }

        [JsonProperty("timezone")]
        public string? Timezone { get; set; }

        [JsonProperty("timezoneOffset")]
        public string? TimezoneOffset { get; set; }

        [JsonProperty("item_search_key")]
        public string? ItemSearchKey { get; set; }

    }


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
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("createdby")]
        public string Createdby { get; set; }

        [JsonProperty("createddatetime")]
        public DateTimeOffset Createddatetime { get; set; }

        [JsonProperty("modifiedby")]
        public object Modifiedby { get; set; }

        [JsonProperty("modifieddatetime")]
        public DateTimeOffset Modifieddatetime { get; set; }

        [JsonProperty("controllersid")]
        public string Controllersid { get; set; }

        [JsonProperty("originapplication")]
        public string Originapplication { get; set; }

        [JsonProperty("postdate")]
        public DateTimeOffset Postdate { get; set; }

        [JsonProperty("rowversion")]
        public long Rowversion { get; set; }

        [JsonProperty("tenantsid")]
        public string Tenantsid { get; set; }

        [JsonProperty("sessionstatus")]
        public long Sessionstatus { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("employeesid")]
        public string Employeesid { get; set; }

        [JsonProperty("employeename")]
        public string Employeename { get; set; }

        [JsonProperty("employeeactive")]
        public bool Employeeactive { get; set; }

        [JsonProperty("employeeissysadmin")]
        public bool Employeeissysadmin { get; set; }

        [JsonProperty("workstationid")]
        public string Workstationid { get; set; }

        [JsonProperty("databasetype")]
        public string Databasetype { get; set; }

        [JsonProperty("seated")]
        public bool Seated { get; set; }

        [JsonProperty("seatsid")]
        public object Seatsid { get; set; }

        [JsonProperty("seatedapp")]
        public string Seatedapp { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("internal")]
        public bool Internal { get; set; }

        [JsonProperty("lasttransaction")]
        public DateTimeOffset Lasttransaction { get; set; }

        [JsonProperty("subsidiarysid")]
        public string Subsidiarysid { get; set; }

        [JsonProperty("storesid")]
        public string Storesid { get; set; }

        [JsonProperty("seasonsid")]
        public string Seasonsid { get; set; }

        [JsonProperty("regionsid")]
        public string Regionsid { get; set; }

        [JsonProperty("districtsid")]
        public string Districtsid { get; set; }

        [JsonProperty("workstationtype")]
        public long Workstationtype { get; set; }

        [JsonProperty("workstation")]
        public string Workstation { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("storeactive")]
        public bool Storeactive { get; set; }

        [JsonProperty("departmentname")]
        public object Departmentname { get; set; }

        [JsonProperty("homeurl")]
        public object Homeurl { get; set; }

        [JsonProperty("xforwardedfor")]
        public object Xforwardedfor { get; set; }

        [JsonProperty("serveraddress")]
        public Uri Serveraddress { get; set; }

        [JsonProperty("imageserveraddress")]
        public Uri Imageserveraddress { get; set; }

        [JsonProperty("oldsbssid")]
        public string Oldsbssid { get; set; }

        [JsonProperty("oldstoresid")]
        public string Oldstoresid { get; set; }

        [JsonProperty("servertimezone")]
        public long Servertimezone { get; set; }

        [JsonProperty("pricelevelsid")]
        public string Pricelevelsid { get; set; }

        [JsonProperty("hisecsbssid")]
        public string Hisecsbssid { get; set; }

        [JsonProperty("hisecemplsid")]
        public string Hisecemplsid { get; set; }

        [JsonProperty("basecurrencycodealpha")]
        public string Basecurrencycodealpha { get; set; }

        [JsonProperty("basecurrencysymbol")]
        public object Basecurrencysymbol { get; set; }

        [JsonProperty("controllernumber")]
        public long Controllernumber { get; set; }

        [JsonProperty("workstationnumber")]
        public long Workstationnumber { get; set; }

        [JsonProperty("subsidiarynumber")]
        public long Subsidiarynumber { get; set; }

        [JsonProperty("storenumber")]
        public long Storenumber { get; set; }

        [JsonProperty("storecode")]
        public string Storecode { get; set; }

        [JsonProperty("subsidiaryname")]
        public string Subsidiaryname { get; set; }

        [JsonProperty("storename")]
        public string Storename { get; set; }

        [JsonProperty("languagesid")]
        public string Languagesid { get; set; }

        [JsonProperty("ispoa")]
        public bool Ispoa { get; set; }

        [JsonProperty("isstore")]
        public bool Isstore { get; set; }

        [JsonProperty("isstandalone")]
        public bool Isstandalone { get; set; }

        [JsonProperty("tillsid")]
        public object Tillsid { get; set; }

        [JsonProperty("drawernumber")]
        public long Drawernumber { get; set; }

        [JsonProperty("employeemaxdiscperc")]
        public long Employeemaxdiscperc { get; set; }

        [JsonProperty("countrycode")]
        public object Countrycode { get; set; }

        [JsonProperty("rpproductcode")]
        public object Rpproductcode { get; set; }

        [JsonProperty("opendrawereventsid")]
        public object Opendrawereventsid { get; set; }

        [JsonProperty("basecurrencysid")]
        public string Basecurrencysid { get; set; }

        [JsonProperty("countrysid")]
        public string Countrysid { get; set; }

        [JsonProperty("statuserrorcode")]
        public long Statuserrorcode { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; set; }

        [JsonProperty("registersid")]
        public string Registersid { get; set; }

        [JsonProperty("registerstate")]
        public long Registerstate { get; set; }

        [JsonProperty("cacheseq")]
        public long Cacheseq { get; set; }

        [JsonProperty("licenseinfo")]
        public Licenseinfo Licenseinfo { get; set; }
    }

    public partial class Licenseinfo
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("productcode")]
        public string Productcode { get; set; }

        [JsonProperty("productversion")]
        public string Productversion { get; set; }

        [JsonProperty("expirationdate")]
        public DateTimeOffset Expirationdate { get; set; }

        [JsonProperty("maxstores")]
        public long Maxstores { get; set; }

        [JsonProperty("maxsites")]
        public long Maxsites { get; set; }

        [JsonProperty("maxseats")]
        public long Maxseats { get; set; }

        [JsonProperty("valid")]
        public bool Valid { get; set; }

        [JsonProperty("fs_syncstatus")]
        public long FsSyncstatus { get; set; }

        [JsonProperty("fs_lastsyncdate")]
        public DateTimeOffset FsLastsyncdate { get; set; }

        [JsonProperty("fs_nextautosyncdate")]
        public DateTimeOffset FsNextautosyncdate { get; set; }

        [JsonProperty("fs_nextforcesyncdate")]
        public DateTimeOffset FsNextforcesyncdate { get; set; }
    }

    public partial class Permissions
    {
        [JsonProperty("inventoryassemblekits")]
        public string Inventoryassemblekits { get; set; }

        [JsonProperty("allowmanualdetax")]
        public string Allowmanualdetax { get; set; }

        [JsonProperty("xzoutforcecloseregister")]
        public string Xzoutforcecloseregister { get; set; }

        [JsonProperty("departmentcreatecopy")]
        public string Departmentcreatecopy { get; set; }

        [JsonProperty("posallowcustomerchangeonfinalizedtran")]
        public string Posallowcustomerchangeonfinalizedtran { get; set; }

        [JsonProperty("autotoolsgeneratepurchaseorders")]
        public string Autotoolsgeneratepurchaseorders { get; set; }

        [JsonProperty("disbursementschangesubsidiary")]
        public string Disbursementschangesubsidiary { get; set; }

        [JsonProperty("purchaseorderchangesubsidiary")]
        public string Purchaseorderchangesubsidiary { get; set; }

        [JsonProperty("pcpromoedit")]
        public string Pcpromoedit { get; set; }

        [JsonProperty("allowbreakingkits")]
        public string Allowbreakingkits { get; set; }

        [JsonProperty("vendorallowmanualentryofudfvalues")]
        public string Vendorallowmanualentryofudfvalues { get; set; }

        [JsonProperty("editcustomerchargebalance")]
        public string Editcustomerchargebalance { get; set; }

        [JsonProperty("transferordercreatecopy")]
        public string Transferordercreatecopy { get; set; }

        [JsonProperty("inventorybreakkits")]
        public string Inventorybreakkits { get; set; }

        [JsonProperty("accessstoreoperations")]
        public string Accessstoreoperations { get; set; }

        [JsonProperty("poedititemnotes")]
        public string Poedititemnotes { get; set; }

        [JsonProperty("inventoryitemeditminmax")]
        public string Inventoryitemeditminmax { get; set; }

        [JsonProperty("poslookupzeropriceditems")]
        public string Poslookupzeropriceditems { get; set; }

        [JsonProperty("vendorcreatecopy")]
        public string Vendorcreatecopy { get; set; }

        [JsonProperty("changecheckinoutdatetime")]
        public string Changecheckinoutdatetime { get; set; }

        [JsonProperty("employeeaccountunlock")]
        public string Employeeaccountunlock { get; set; }

        [JsonProperty("editsubsidiary")]
        public string Editsubsidiary { get; set; }

        [JsonProperty("tenderstakecod")]
        public string Tenderstakecod { get; set; }

        [JsonProperty("pizoneunmerge")]
        public string Pizoneunmerge { get; set; }

        [JsonProperty("regionalinventoryeditdcsregionalflag")]
        public string Regionalinventoryeditdcsregionalflag { get; set; }

        [JsonProperty("poeditnumber")]
        public string Poeditnumber { get; set; }

        [JsonProperty("custactivatedeactivate")]
        public string Custactivatedeactivate { get; set; }

        [JsonProperty("pmchangesubsidiary")]
        public string Pmchangesubsidiary { get; set; }

        [JsonProperty("posoverridereturntenderrestriction")]
        public string Posoverridereturntenderrestriction { get; set; }

        [JsonProperty("merchandiseprinttag")]
        public string Merchandiseprinttag { get; set; }

        [JsonProperty("posaddprepaidgiftcard")]
        public string Posaddprepaidgiftcard { get; set; }

        [JsonProperty("voucherschangesubsidiary")]
        public string Voucherschangesubsidiary { get; set; }

        [JsonProperty("lnallowusewhennotininventory")]
        public string Lnallowusewhennotininventory { get; set; }

        [JsonProperty("inventoryitemcreatecopy")]
        public string Inventoryitemcreatecopy { get; set; }

        [JsonProperty("allowothertendersforreturns")]
        public string Allowothertendersforreturns { get; set; }

        [JsonProperty("piaccess")]
        public string Piaccess { get; set; }

        [JsonProperty("tendersgivecheck")]
        public string Tendersgivecheck { get; set; }

        [JsonProperty("seeloyaltypoints")]
        public string Seeloyaltypoints { get; set; }

        [JsonProperty("regionalinventoryeditinvnregionalflag")]
        public string Regionalinventoryeditinvnregionalflag { get; set; }

        [JsonProperty("tendersgivedebitcard")]
        public string Tendersgivedebitcard { get; set; }

        [JsonProperty("custallowdeactivatewithstorecredit")]
        public string Custallowdeactivatewithstorecredit { get; set; }

        [JsonProperty("asnchangesubsidiary")]
        public string Asnchangesubsidiary { get; set; }

        [JsonProperty("poschangetaxarea")]
        public string Poschangetaxarea { get; set; }

        [JsonProperty("autotoolsconfigureautominmax")]
        public string Autotoolsconfigureautominmax { get; set; }

        [JsonProperty("voiditem")]
        public string Voiditem { get; set; }

        [JsonProperty("piprintzonesheet")]
        public string Piprintzonesheet { get; set; }

        [JsonProperty("tendersgivegiftcard")]
        public string Tendersgivegiftcard { get; set; }

        [JsonProperty("asncreatecopy")]
        public string Asncreatecopy { get; set; }

        [JsonProperty("editformerposassociate")]
        public string Editformerposassociate { get; set; }

        [JsonProperty("purchaseorderchangestore")]
        public string Purchaseorderchangestore { get; set; }

        [JsonProperty("importexportallowdataexport")]
        public string Importexportallowdataexport { get; set; }

        [JsonProperty("transferverificationaccess")]
        public string Transferverificationaccess { get; set; }

        [JsonProperty("pieditpizone")]
        public string Pieditpizone { get; set; }

        [JsonProperty("asndeactivatedelete")]
        public string Asndeactivatedelete { get; set; }

        [JsonProperty("vouchersallowmissingpackages")]
        public string Vouchersallowmissingpackages { get; set; }

        [JsonProperty("slaccesssecureslonmemos")]
        public string Slaccesssecureslonmemos { get; set; }

        [JsonProperty("slaccesssecureslonpostransactions")]
        public string Slaccesssecureslonpostransactions { get; set; }

        [JsonProperty("vendoraccess")]
        public string Vendoraccess { get; set; }

        [JsonProperty("vendoredit")]
        public string Vendoredit { get; set; }

        [JsonProperty("voucherseditdocumentdate")]
        public string Voucherseditdocumentdate { get; set; }

        [JsonProperty("tendersreturncharge")]
        public string Tendersreturncharge { get; set; }

        [JsonProperty("acaccesscorporateregions")]
        public string Acaccesscorporateregions { get; set; }

        [JsonProperty("pizonemerge")]
        public string Pizonemerge { get; set; }

        [JsonProperty("xzoutopenregister")]
        public string Xzoutopenregister { get; set; }

        [JsonProperty("acaccesstouchmenu")]
        public string Acaccesstouchmenu { get; set; }

        [JsonProperty("adjaccessadjmemolookup")]
        public string Adjaccessadjmemolookup { get; set; }

        [JsonProperty("acaccesscustomizations")]
        public string Acaccesscustomizations { get; set; }

        [JsonProperty("tendersgivecash")]
        public string Tendersgivecash { get; set; }

        [JsonProperty("vendoractivatedeactivate")]
        public string Vendoractivatedeactivate { get; set; }

        [JsonProperty("asnedititemqty")]
        public string Asnedititemqty { get; set; }

        [JsonProperty("removediscounts")]
        public string Removediscounts { get; set; }

        [JsonProperty("adjmemoscreatecostmemo")]
        public string Adjmemoscreatecostmemo { get; set; }

        [JsonProperty("disbursementschangestore")]
        public string Disbursementschangestore { get; set; }

        [JsonProperty("inventoryitemeditonhandqty")]
        public string Inventoryitemeditonhandqty { get; set; }

        [JsonProperty("tendersreturndebitcard")]
        public string Tendersreturndebitcard { get; set; }

        [JsonProperty("inventoryitemassignserialtype")]
        public string Inventoryitemassignserialtype { get; set; }

        [JsonProperty("poschangegiftcardexpiredate")]
        public string Poschangegiftcardexpiredate { get; set; }

        [JsonProperty("adjchangesubsidiary")]
        public string Adjchangesubsidiary { get; set; }

        [JsonProperty("xzouteditzoutdate")]
        public string Xzouteditzoutdate { get; set; }

        [JsonProperty("posallowsalesfee")]
        public string Posallowsalesfee { get; set; }

        [JsonProperty("autotoolsrunautoto")]
        public string Autotoolsrunautoto { get; set; }

        [JsonProperty("managecurrencies")]
        public string Managecurrencies { get; set; }

        [JsonProperty("xzoutcloseregister")]
        public string Xzoutcloseregister { get; set; }

        [JsonProperty("vendorinvoicecreate")]
        public string Vendorinvoicecreate { get; set; }

        [JsonProperty("inventoryitemedit")]
        public string Inventoryitemedit { get; set; }

        [JsonProperty("asnedit")]
        public string Asnedit { get; set; }

        [JsonProperty("vouchersedit")]
        public string Vouchersedit { get; set; }

        [JsonProperty("closeorder")]
        public string Closeorder { get; set; }

        [JsonProperty("inventoryitemalloweditstoreohqtywithoutenteringsnorlns")]
        public string Inventoryitemalloweditstoreohqtywithoutenteringsnorlns { get; set; }

        [JsonProperty("snallowusewhenalreadyreceived")]
        public string Snallowusewhenalreadyreceived { get; set; }

        [JsonProperty("seeitemcost")]
        public string Seeitemcost { get; set; }

        [JsonProperty("tendersgivecharge")]
        public string Tendersgivecharge { get; set; }

        [JsonProperty("pieditsheet")]
        public string Pieditsheet { get; set; }

        [JsonProperty("createsubsidiary")]
        public string Createsubsidiary { get; set; }

        [JsonProperty("vendorinvoiceapprove")]
        public string Vendorinvoiceapprove { get; set; }

        [JsonProperty("tendersreturngiftcard")]
        public string Tendersreturngiftcard { get; set; }

        [JsonProperty("transferorderactivatedeactivate")]
        public string Transferorderactivatedeactivate { get; set; }

        [JsonProperty("poseditpending")]
        public string Poseditpending { get; set; }

        [JsonProperty("acaccesstransfers")]
        public string Acaccesstransfers { get; set; }

        [JsonProperty("transferorderedit")]
        public string Transferorderedit { get; set; }

        [JsonProperty("slchangeslonslips")]
        public string Slchangeslonslips { get; set; }

        [JsonProperty("posallownegativesalesfee")]
        public string Posallownegativesalesfee { get; set; }

        [JsonProperty("asnchangestore")]
        public string Asnchangestore { get; set; }

        [JsonProperty("tenderstakegiftcertificate")]
        public string Tenderstakegiftcertificate { get; set; }

        [JsonProperty("customerseditcustomer")]
        public string Customerseditcustomer { get; set; }

        [JsonProperty("allowvoidtender")]
        public string Allowvoidtender { get; set; }

        [JsonProperty("acaccesswsdetails")]
        public string Acaccesswsdetails { get; set; }

        [JsonProperty("tendersreturncheck")]
        public string Tendersreturncheck { get; set; }

        [JsonProperty("customerchangesubsidiary")]
        public string Customerchangesubsidiary { get; set; }

        [JsonProperty("autotoolsgenerateautoto")]
        public string Autotoolsgenerateautoto { get; set; }

        [JsonProperty("acaccesssequencing")]
        public string Acaccesssequencing { get; set; }

        [JsonProperty("tenderstakestorecredit")]
        public string Tenderstakestorecredit { get; set; }

        [JsonProperty("sendsaleadditem")]
        public string Sendsaleadditem { get; set; }

        [JsonProperty("snallowusewhenonanotherorderitem")]
        public string Snallowusewhenonanotherorderitem { get; set; }

        [JsonProperty("posaccesstransactionlookup")]
        public string Posaccesstransactionlookup { get; set; }

        [JsonProperty("transferorderprintheld")]
        public string Transferorderprintheld { get; set; }

        [JsonProperty("pcpromoaccess")]
        public string Pcpromoaccess { get; set; }

        [JsonProperty("vendorchangesubsidiary")]
        public string Vendorchangesubsidiary { get; set; }

        [JsonProperty("manageexchangerates")]
        public string Manageexchangerates { get; set; }

        [JsonProperty("tendersreturngiftcertificate")]
        public string Tendersreturngiftcertificate { get; set; }

        [JsonProperty("setcustomerarflag")]
        public string Setcustomerarflag { get; set; }

        [JsonProperty("adjmemosprintheld")]
        public string Adjmemosprintheld { get; set; }

        [JsonProperty("acaccesslanguageandlocale")]
        public string Acaccesslanguageandlocale { get; set; }

        [JsonProperty("posaddstoredgiftcard")]
        public string Posaddstoredgiftcard { get; set; }

        [JsonProperty("slaccesssecureslonvouchers")]
        public string Slaccesssecureslonvouchers { get; set; }

        [JsonProperty("tenderstakecustomtender")]
        public string Tenderstakecustomtender { get; set; }

        [JsonProperty("picreatepizone")]
        public string Picreatepizone { get; set; }

        [JsonProperty("inventoryitemeditprices")]
        public string Inventoryitemeditprices { get; set; }

        [JsonProperty("acctlinkoptionspreferences")]
        public string Acctlinkoptionspreferences { get; set; }

        [JsonProperty("groupmodifypermissions")]
        public string Groupmodifypermissions { get; set; }

        [JsonProperty("piupdatesheet")]
        public string Piupdatesheet { get; set; }

        [JsonProperty("acaccesspurchasing")]
        public string Acaccesspurchasing { get; set; }

        [JsonProperty("poschangestore")]
        public string Poschangestore { get; set; }

        [JsonProperty("pcpromoactivate")]
        public string Pcpromoactivate { get; set; }

        [JsonProperty("groupassignexculdegroupmember")]
        public string Groupassignexculdegroupmember { get; set; }

        [JsonProperty("acctlinklogaccess")]
        public string Acctlinklogaccess { get; set; }

        [JsonProperty("poedit")]
        public string Poedit { get; set; }

        [JsonProperty("posedititemnotes")]
        public string Posedititemnotes { get; set; }

        [JsonProperty("loyaltyprogramcreateedit")]
        public string Loyaltyprogramcreateedit { get; set; }

        [JsonProperty("autotoolsaccessautopurchasing")]
        public string Autotoolsaccessautopurchasing { get; set; }

        [JsonProperty("acaccessmerchandise")]
        public string Acaccessmerchandise { get; set; }

        [JsonProperty("transferscreateslip")]
        public string Transferscreateslip { get; set; }

        [JsonProperty("transferslipeditdocumentdate")]
        public string Transferslipeditdocumentdate { get; set; }

        [JsonProperty("piprintsheet")]
        public string Piprintsheet { get; set; }

        [JsonProperty("transferseditformerslip")]
        public string Transferseditformerslip { get; set; }

        [JsonProperty("inventorymanagepackages")]
        public string Inventorymanagepackages { get; set; }

        [JsonProperty("custchangepricelevel")]
        public string Custchangepricelevel { get; set; }

        [JsonProperty("vouchersprintheld")]
        public string Vouchersprintheld { get; set; }

        [JsonProperty("transferseditreceiveditems")]
        public string Transferseditreceiveditems { get; set; }

        [JsonProperty("tendersgivegiftcertificate")]
        public string Tendersgivegiftcertificate { get; set; }

        [JsonProperty("editlockedloyaltylevel")]
        public string Editlockedloyaltylevel { get; set; }

        [JsonProperty("adjmemosupdatememo")]
        public string Adjmemosupdatememo { get; set; }

        [JsonProperty("slchangeslonpostransactions")]
        public string Slchangeslonpostransactions { get; set; }

        [JsonProperty("employeemanagementdeletegroups")]
        public string Employeemanagementdeletegroups { get; set; }

        [JsonProperty("acaccessgridformats")]
        public string Acaccessgridformats { get; set; }

        [JsonProperty("vouchersreturnvoucher")]
        public string Vouchersreturnvoucher { get; set; }

        [JsonProperty("licensingdeactivation")]
        public string Licensingdeactivation { get; set; }

        [JsonProperty("departmentchangesubsidiary")]
        public string Departmentchangesubsidiary { get; set; }

        [JsonProperty("editformerpostrackingnumber")]
        public string Editformerpostrackingnumber { get; set; }

        [JsonProperty("adjmemoscreateqtymemo")]
        public string Adjmemoscreateqtymemo { get; set; }

        [JsonProperty("transferorderchangestore")]
        public string Transferorderchangestore { get; set; }

        [JsonProperty("pimodifyzonecounts")]
        public string Pimodifyzonecounts { get; set; }

        [JsonProperty("pcpromocreate")]
        public string Pcpromocreate { get; set; }

        [JsonProperty("customersselectudfvalues")]
        public string Customersselectudfvalues { get; set; }

        [JsonProperty("slchangeslonmemos")]
        public string Slchangeslonmemos { get; set; }

        [JsonProperty("sopmperformupdate")]
        public string Sopmperformupdate { get; set; }

        [JsonProperty("disbursementsprintupdated")]
        public string Disbursementsprintupdated { get; set; }

        [JsonProperty("autotoolscalculateautopurchasing")]
        public string Autotoolscalculateautopurchasing { get; set; }

        [JsonProperty("sopmseesubsidiaries")]
        public string Sopmseesubsidiaries { get; set; }

        [JsonProperty("adjchangestore")]
        public string Adjchangestore { get; set; }

        [JsonProperty("transferorderaccess")]
        public string Transferorderaccess { get; set; }

        [JsonProperty("merchandiseaccessitemhistory")]
        public string Merchandiseaccessitemhistory { get; set; }

        [JsonProperty("tendersgivecod")]
        public string Tendersgivecod { get; set; }

        [JsonProperty("checkcentralcreditbal")]
        public string Checkcentralcreditbal { get; set; }

        [JsonProperty("fulfillsendsale")]
        public string Fulfillsendsale { get; set; }

        [JsonProperty("customerseditinfomarkfields")]
        public string Customerseditinfomarkfields { get; set; }

        [JsonProperty("acaccessstoredetails")]
        public string Acaccessstoredetails { get; set; }

        [JsonProperty("createstore")]
        public string Createstore { get; set; }

        [JsonProperty("xzouteditzoutfilter")]
        public string Xzouteditzoutfilter { get; set; }

        [JsonProperty("posallownegativeorderdeposits")]
        public string Posallownegativeorderdeposits { get; set; }

        [JsonProperty("tendersgivecustomtender")]
        public string Tendersgivecustomtender { get; set; }

        [JsonProperty("tendersreturncentralgiftcard")]
        public string Tendersreturncentralgiftcard { get; set; }

        [JsonProperty("asneditvoucherprice")]
        public string Asneditvoucherprice { get; set; }

        [JsonProperty("transferverificationmanual")]
        public string Transferverificationmanual { get; set; }

        [JsonProperty("acaccesssystem")]
        public string Acaccesssystem { get; set; }

        [JsonProperty("tendersreturncod")]
        public string Tendersreturncod { get; set; }

        [JsonProperty("transferverificationchangesubsidiary")]
        public string Transferverificationchangesubsidiary { get; set; }

        [JsonProperty("timeclockdeletecheckout")]
        public string Timeclockdeletecheckout { get; set; }

        [JsonProperty("piupdatestartquantity")]
        public string Piupdatestartquantity { get; set; }

        [JsonProperty("editformerdisbursement")]
        public string Editformerdisbursement { get; set; }

        [JsonProperty("vouchersreversevoucher")]
        public string Vouchersreversevoucher { get; set; }

        [JsonProperty("timeclockchangesubsidiary")]
        public string Timeclockchangesubsidiary { get; set; }

        [JsonProperty("xzoutchangesubsidiary")]
        public string Xzoutchangesubsidiary { get; set; }

        [JsonProperty("tenderstakecheck")]
        public string Tenderstakecheck { get; set; }

        [JsonProperty("customerscreatecustomer")]
        public string Customerscreatecustomer { get; set; }

        [JsonProperty("poschangepricelevel")]
        public string Poschangepricelevel { get; set; }

        [JsonProperty("acaccesshardware")]
        public string Acaccesshardware { get; set; }

        [JsonProperty("asneditvouchercost")]
        public string Asneditvouchercost { get; set; }

        [JsonProperty("deletezone")]
        public string Deletezone { get; set; }

        [JsonProperty("poprint")]
        public string Poprint { get; set; }

        [JsonProperty("poinactivatedelete")]
        public string Poinactivatedelete { get; set; }

        [JsonProperty("adjmemoscreatepricememo")]
        public string Adjmemoscreatepricememo { get; set; }

        [JsonProperty("inventoryitemalloweditserialandlotnumber")]
        public string Inventoryitemalloweditserialandlotnumber { get; set; }

        [JsonProperty("sopmeditmarkdowns")]
        public string Sopmeditmarkdowns { get; set; }

        [JsonProperty("voucherseditpending")]
        public string Voucherseditpending { get; set; }

        [JsonProperty("employeemanagementchangetill")]
        public string Employeemanagementchangetill { get; set; }

        [JsonProperty("poschangeorderfulfillmentstore")]
        public string Poschangeorderfulfillmentstore { get; set; }

        [JsonProperty("tendersgivestorecredit")]
        public string Tendersgivestorecredit { get; set; }

        [JsonProperty("poinactiveaccess")]
        public string Poinactiveaccess { get; set; }

        [JsonProperty("createpostransaction")]
        public string Createpostransaction { get; set; }

        [JsonProperty("editcustomersloyaltylevel")]
        public string Editcustomersloyaltylevel { get; set; }

        [JsonProperty("tenderstakegiftcard")]
        public string Tenderstakegiftcard { get; set; }

        [JsonProperty("voucherseditprice")]
        public string Voucherseditprice { get; set; }

        [JsonProperty("accessreceivingarea")]
        public string Accessreceivingarea { get; set; }

        [JsonProperty("accessemployeemanagement")]
        public string Accessemployeemanagement { get; set; }

        [JsonProperty("tenderstakecentralgiftcard")]
        public string Tenderstakecentralgiftcard { get; set; }

        [JsonProperty("acctlinkoptionsinitialize")]
        public string Acctlinkoptionsinitialize { get; set; }

        [JsonProperty("timeclockcheckinout")]
        public string Timeclockcheckinout { get; set; }

        [JsonProperty("piactivatesheet")]
        public string Piactivatesheet { get; set; }

        [JsonProperty("acaccesstransactions")]
        public string Acaccesstransactions { get; set; }

        [JsonProperty("licensingkillsession")]
        public string Licensingkillsession { get; set; }

        [JsonProperty("batchreceivingchangesubsidiary")]
        public string Batchreceivingchangesubsidiary { get; set; }

        [JsonProperty("xzoutaccessformeraudit")]
        public string Xzoutaccessformeraudit { get; set; }

        [JsonProperty("accessmerchandisearea")]
        public string Accessmerchandisearea { get; set; }

        [JsonProperty("transferseditformerassociate")]
        public string Transferseditformerassociate { get; set; }

        [JsonProperty("allowupdateonlyatpos")]
        public string Allowupdateonlyatpos { get; set; }

        [JsonProperty("manuallyadjustloyaltypoints")]
        public string Manuallyadjustloyaltypoints { get; set; }

        [JsonProperty("promotionsenablebroadcast")]
        public string Promotionsenablebroadcast { get; set; }

        [JsonProperty("autotoolscalculateautominmax")]
        public string Autotoolscalculateautominmax { get; set; }

        [JsonProperty("inventoryitemeditcost")]
        public string Inventoryitemeditcost { get; set; }

        [JsonProperty("poschangesubsidiary")]
        public string Poschangesubsidiary { get; set; }

        [JsonProperty("tendersreturncustomtender")]
        public string Tendersreturncustomtender { get; set; }

        [JsonProperty("acaccessemployee")]
        public string Acaccessemployee { get; set; }

        [JsonProperty("adjmemosallowbypassfullctlln")]
        public string Adjmemosallowbypassfullctlln { get; set; }

        [JsonProperty("autotoolsaccessautoutilities")]
        public string Autotoolsaccessautoutilities { get; set; }

        [JsonProperty("poaccess")]
        public string Poaccess { get; set; }

        [JsonProperty("regionalinventoryeditvendorregionalflag")]
        public string Regionalinventoryeditvendorregionalflag { get; set; }

        [JsonProperty("editemployeecheckinout")]
        public string Editemployeecheckinout { get; set; }

        [JsonProperty("acctlinkpostingaccess")]
        public string Acctlinkpostingaccess { get; set; }

        [JsonProperty("xzoutrunxoutreport")]
        public string Xzoutrunxoutreport { get; set; }

        [JsonProperty("autotoolsconfigureautopurchasing")]
        public string Autotoolsconfigureautopurchasing { get; set; }

        [JsonProperty("posspecialorderadditem")]
        public string Posspecialorderadditem { get; set; }

        [JsonProperty("adjmemosunholdmemo")]
        public string Adjmemosunholdmemo { get; set; }

        [JsonProperty("editformerposdetails")]
        public string Editformerposdetails { get; set; }

        [JsonProperty("inventoryallowmanualentryofudfvalues")]
        public string Inventoryallowmanualentryofudfvalues { get; set; }

        [JsonProperty("pcpromodeactivate")]
        public string Pcpromodeactivate { get; set; }

        [JsonProperty("asnchangeassociate")]
        public string Asnchangeassociate { get; set; }

        [JsonProperty("employeechangetill")]
        public string Employeechangetill { get; set; }

        [JsonProperty("transferslipreverseslip")]
        public string Transferslipreverseslip { get; set; }

        [JsonProperty("transferscopyformerslip")]
        public string Transferscopyformerslip { get; set; }

        [JsonProperty("promotionscopy")]
        public string Promotionscopy { get; set; }

        [JsonProperty("acaccessconnectionmanager")]
        public string Acaccessconnectionmanager { get; set; }

        [JsonProperty("poschangeassociate")]
        public string Poschangeassociate { get; set; }

        [JsonProperty("acaccessdaysintransit")]
        public string Acaccessdaysintransit { get; set; }

        [JsonProperty("departmentedit")]
        public string Departmentedit { get; set; }

        [JsonProperty("adjmemosallowbypassfullctlsn")]
        public string Adjmemosallowbypassfullctlsn { get; set; }

        [JsonProperty("adjmemosprintupdated")]
        public string Adjmemosprintupdated { get; set; }

        [JsonProperty("tendersreturncash")]
        public string Tendersreturncash { get; set; }

        [JsonProperty("pimergeunmergezones")]
        public string Pimergeunmergezones { get; set; }

        [JsonProperty("inventoryitemactivate")]
        public string Inventoryitemactivate { get; set; }

        [JsonProperty("givedocdiscount")]
        public string Givedocdiscount { get; set; }

        [JsonProperty("pocreatecopy")]
        public string Pocreatecopy { get; set; }

        [JsonProperty("inventoryitemassignlottype")]
        public string Inventoryitemassignlottype { get; set; }

        [JsonProperty("xzoutchangestore")]
        public string Xzoutchangestore { get; set; }

        [JsonProperty("timeclockchangestore")]
        public string Timeclockchangestore { get; set; }

        [JsonProperty("posspecialorderclose")]
        public string Posspecialorderclose { get; set; }

        [JsonProperty("posholdunholdtransaction")]
        public string Posholdunholdtransaction { get; set; }

        [JsonProperty("copypostransactionreceipts")]
        public string Copypostransactionreceipts { get; set; }

        [JsonProperty("asnprint")]
        public string Asnprint { get; set; }

        [JsonProperty("poseditdocumentdate")]
        public string Poseditdocumentdate { get; set; }

        [JsonProperty("tendersgivecreditcard")]
        public string Tendersgivecreditcard { get; set; }

        [JsonProperty("runreportdesigner")]
        public string Runreportdesigner { get; set; }

        [JsonProperty("transferorderchangesubsidiary")]
        public string Transferorderchangesubsidiary { get; set; }

        [JsonProperty("voucherscopy")]
        public string Voucherscopy { get; set; }

        [JsonProperty("employeeeditcreate")]
        public string Employeeeditcreate { get; set; }

        [JsonProperty("asnnegativequantity")]
        public string Asnnegativequantity { get; set; }

        [JsonProperty("poschangeitemtaxcode")]
        public string Poschangeitemtaxcode { get; set; }

        [JsonProperty("inventoryitemeditordercost")]
        public string Inventoryitemeditordercost { get; set; }

        [JsonProperty("posallowupdatewithoutitems")]
        public string Posallowupdatewithoutitems { get; set; }

        [JsonProperty("ttkallowaccess")]
        public string Ttkallowaccess { get; set; }

        [JsonProperty("acctlinkbatchespost")]
        public string Acctlinkbatchespost { get; set; }

        [JsonProperty("viewitemdetails")]
        public string Viewitemdetails { get; set; }

        [JsonProperty("picreatesheet")]
        public string Picreatesheet { get; set; }

        [JsonProperty("transferslipeditpending")]
        public string Transferslipeditpending { get; set; }

        [JsonProperty("voucherscreate")]
        public string Voucherscreate { get; set; }

        [JsonProperty("posprintupdated")]
        public string Posprintupdated { get; set; }

        [JsonProperty("posadvanceditemlookup")]
        public string Posadvanceditemlookup { get; set; }

        [JsonProperty("tenderstakeforeigncurrency")]
        public string Tenderstakeforeigncurrency { get; set; }

        [JsonProperty("tendersreturnstorecredit")]
        public string Tendersreturnstorecredit { get; set; }

        [JsonProperty("vendorinvoiceaccess")]
        public string Vendorinvoiceaccess { get; set; }

        [JsonProperty("licensingchangeallocation")]
        public string Licensingchangeallocation { get; set; }

        [JsonProperty("inventoryitemaccess")]
        public string Inventoryitemaccess { get; set; }

        [JsonProperty("groupmanagedefinitions")]
        public string Groupmanagedefinitions { get; set; }

        [JsonProperty("tendersgiveforeigncurrency")]
        public string Tendersgiveforeigncurrency { get; set; }

        [JsonProperty("acaccessdatatypes")]
        public string Acaccessdatatypes { get; set; }

        [JsonProperty("allownegcentralcreditbal")]
        public string Allownegcentralcreditbal { get; set; }

        [JsonProperty("transferslipprintheld")]
        public string Transferslipprintheld { get; set; }

        [JsonProperty("acctlinkbatchesread")]
        public string Acctlinkbatchesread { get; set; }

        [JsonProperty("slaccesssecureslonslips")]
        public string Slaccesssecureslonslips { get; set; }

        [JsonProperty("orderitem")]
        public string Orderitem { get; set; }

        [JsonProperty("unholdotherstransaction")]
        public string Unholdotherstransaction { get; set; }

        [JsonProperty("sopmaccessmanager")]
        public string Sopmaccessmanager { get; set; }

        [JsonProperty("issuetaxrebate")]
        public string Issuetaxrebate { get; set; }

        [JsonProperty("autotoolsaccessautominmax")]
        public string Autotoolsaccessautominmax { get; set; }

        [JsonProperty("customerseditaddresses")]
        public string Customerseditaddresses { get; set; }

        [JsonProperty("exceedchargelimit")]
        public string Exceedchargelimit { get; set; }

        [JsonProperty("tendersreturncreditcard")]
        public string Tendersreturncreditcard { get; set; }

        [JsonProperty("vouchersprintupdated")]
        public string Vouchersprintupdated { get; set; }

        [JsonProperty("asnflagpackagesreceived")]
        public string Asnflagpackagesreceived { get; set; }

        [JsonProperty("tendersgivecentralgiftcard")]
        public string Tendersgivecentralgiftcard { get; set; }

        [JsonProperty("adjustmentmemoseditdocumentdate")]
        public string Adjustmentmemoseditdocumentdate { get; set; }

        [JsonProperty("poprintheld")]
        public string Poprintheld { get; set; }

        [JsonProperty("checkcentralgiftcardbal")]
        public string Checkcentralgiftcardbal { get; set; }

        [JsonProperty("acctlinkwizardaccess")]
        public string Acctlinkwizardaccess { get; set; }

        [JsonProperty("changeopt-instatus")]
        public string ChangeoptInstatus { get; set; }

        [JsonProperty("posalloworderfee")]
        public string Posalloworderfee { get; set; }

        [JsonProperty("importexportallowdataimport")]
        public string Importexportallowdataimport { get; set; }

        [JsonProperty("inventoryitemdeactivate")]
        public string Inventoryitemdeactivate { get; set; }

        [JsonProperty("asngeneratereceivingvoucher")]
        public string Asngeneratereceivingvoucher { get; set; }

        [JsonProperty("employeechangesubsidiary")]
        public string Employeechangesubsidiary { get; set; }

        [JsonProperty("posreprintfinalizedreceipt")]
        public string Posreprintfinalizedreceipt { get; set; }

        [JsonProperty("pimodifyquantity")]
        public string Pimodifyquantity { get; set; }

        [JsonProperty("posallowprintinghelddocuments")]
        public string Posallowprintinghelddocuments { get; set; }

        [JsonProperty("transferorderprint")]
        public string Transferorderprint { get; set; }

        [JsonProperty("xzoutmodifyworkstation")]
        public string Xzoutmodifyworkstation { get; set; }

        [JsonProperty("acaccesstaxes")]
        public string Acaccesstaxes { get; set; }

        [JsonProperty("slchangeslonvouchers")]
        public string Slchangeslonvouchers { get; set; }

        [JsonProperty("removeitemfromposdocument")]
        public string Removeitemfromposdocument { get; set; }

        [JsonProperty("giveitemdiscount")]
        public string Giveitemdiscount { get; set; }

        [JsonProperty("accessadminconsole")]
        public string Accessadminconsole { get; set; }

        [JsonProperty("xzouteditxoutfilter")]
        public string Xzouteditxoutfilter { get; set; }

        [JsonProperty("inventoryitemchangesubsidiary")]
        public string Inventoryitemchangesubsidiary { get; set; }

        [JsonProperty("editformerposcustomer")]
        public string Editformerposcustomer { get; set; }

        [JsonProperty("xzoutaccessformerzout")]
        public string Xzoutaccessformerzout { get; set; }

        [JsonProperty("transferslipchangestore")]
        public string Transferslipchangestore { get; set; }

        [JsonProperty("adjmemosreversememo")]
        public string Adjmemosreversememo { get; set; }

        [JsonProperty("voucherschangeassociate")]
        public string Voucherschangeassociate { get; set; }

        [JsonProperty("transferslipprintupdated")]
        public string Transferslipprintupdated { get; set; }

        [JsonProperty("acaccessseason")]
        public string Acaccessseason { get; set; }

        [JsonProperty("poenterfreeformemailaddress")]
        public string Poenterfreeformemailaddress { get; set; }

        [JsonProperty("inventorypromotespecialorderitem")]
        public string Inventorypromotespecialorderitem { get; set; }

        [JsonProperty("adjmemoseditformermemo")]
        public string Adjmemoseditformermemo { get; set; }

        [JsonProperty("opencashdrawer")]
        public string Opencashdrawer { get; set; }

        [JsonProperty("transfersallowhold")]
        public string Transfersallowhold { get; set; }

        [JsonProperty("timeclockdeletecheckin")]
        public string Timeclockdeletecheckin { get; set; }

        [JsonProperty("autotoolsaccessautoto")]
        public string Autotoolsaccessautoto { get; set; }

        [JsonProperty("tenderstakedebitcard")]
        public string Tenderstakedebitcard { get; set; }

        [JsonProperty("acaccesscustomers")]
        public string Acaccesscustomers { get; set; }

        [JsonProperty("departmentaccess")]
        public string Departmentaccess { get; set; }

        [JsonProperty("acctlinkbatchesview")]
        public string Acctlinkbatchesview { get; set; }

        [JsonProperty("adjustmentmemoseditpending")]
        public string Adjustmentmemoseditpending { get; set; }

        [JsonProperty("sopmdeletemarkdowns")]
        public string Sopmdeletemarkdowns { get; set; }

        [JsonProperty("voucherseditcost")]
        public string Voucherseditcost { get; set; }

        [JsonProperty("acctlinkbatchesdelete")]
        public string Acctlinkbatchesdelete { get; set; }

        [JsonProperty("autotoolsconfigureautoto")]
        public string Autotoolsconfigureautoto { get; set; }

        [JsonProperty("disbursementsaccessdisbursementlookup")]
        public string Disbursementsaccessdisbursementlookup { get; set; }

        [JsonProperty("commonskipavailabileqtycheck")]
        public string Commonskipavailabileqtycheck { get; set; }

        [JsonProperty("vendorinvoiceedit")]
        public string Vendorinvoiceedit { get; set; }

        [JsonProperty("snallowusebeforereceived")]
        public string Snallowusebeforereceived { get; set; }

        [JsonProperty("createdisbursement")]
        public string Createdisbursement { get; set; }

        [JsonProperty("tenderstakecreditcard")]
        public string Tenderstakecreditcard { get; set; }

        [JsonProperty("acaccesssublocations")]
        public string Acaccesssublocations { get; set; }

        [JsonProperty("editcustomerchargelimit")]
        public string Editcustomerchargelimit { get; set; }

        [JsonProperty("poeditponotes")]
        public string Poeditponotes { get; set; }

        [JsonProperty("changetaxrebateamount")]
        public string Changetaxrebateamount { get; set; }

        [JsonProperty("editformerpostransaction")]
        public string Editformerpostransaction { get; set; }

        [JsonProperty("tenderstakecash")]
        public string Tenderstakecash { get; set; }

        [JsonProperty("acctlinkbatchesupdate")]
        public string Acctlinkbatchesupdate { get; set; }

        [JsonProperty("closesendsale")]
        public string Closesendsale { get; set; }

        [JsonProperty("inventorymanagekits")]
        public string Inventorymanagekits { get; set; }

        [JsonProperty("resetpassword")]
        public string Resetpassword { get; set; }

        [JsonProperty("tenderstakecharge")]
        public string Tenderstakecharge { get; set; }

        [JsonProperty("acaccessthemesandlayouts")]
        public string Acaccessthemesandlayouts { get; set; }

        [JsonProperty("asnprintheld")]
        public string Asnprintheld { get; set; }

        [JsonProperty("piexportsheet")]
        public string Piexportsheet { get; set; }

        [JsonProperty("vendorinvoicedelete")]
        public string Vendorinvoicedelete { get; set; }

        [JsonProperty("posaccesspendingtransactions")]
        public string Posaccesspendingtransactions { get; set; }

        [JsonProperty("vendorinvoicechangesubsidiary")]
        public string Vendorinvoicechangesubsidiary { get; set; }

        [JsonProperty("poscanceltransaction")]
        public string Poscanceltransaction { get; set; }

        [JsonProperty("changecheckinoutemployee")]
        public string Changecheckinoutemployee { get; set; }

        [JsonProperty("posaddcentralgiftcard")]
        public string Posaddcentralgiftcard { get; set; }

        [JsonProperty("promotionsdelete")]
        public string Promotionsdelete { get; set; }

        [JsonProperty("returnitem")]
        public string Returnitem { get; set; }

        [JsonProperty("transfersaccess")]
        public string Transfersaccess { get; set; }

        [JsonProperty("slmanagesublocations")]
        public string Slmanagesublocations { get; set; }

        [JsonProperty("poschangeitemtaxamount")]
        public string Poschangeitemtaxamount { get; set; }

        [JsonProperty("xzoutauditregister")]
        public string Xzoutauditregister { get; set; }

        [JsonProperty("voucherschangestore")]
        public string Voucherschangestore { get; set; }

        [JsonProperty("transferslipchangesubsidiary")]
        public string Transferslipchangesubsidiary { get; set; }

        [JsonProperty("vouchershold")]
        public string Vouchershold { get; set; }

        [JsonProperty("adjmemosprintmemo")]
        public string Adjmemosprintmemo { get; set; }

        [JsonProperty("returnitemwithoutsource")]
        public string Returnitemwithoutsource { get; set; }

        [JsonProperty("poschangecashier")]
        public string Poschangecashier { get; set; }

        [JsonProperty("autotoolsupdateautominmax")]
        public string Autotoolsupdateautominmax { get; set; }

        [JsonProperty("posspecialorderfulfill")]
        public string Posspecialorderfulfill { get; set; }

        [JsonProperty("customerallowmanualentryofudfvalues")]
        public string Customerallowmanualentryofudfvalues { get; set; }

        [JsonProperty("xzoutfinalizezoutreport")]
        public string Xzoutfinalizezoutreport { get; set; }

        [JsonProperty("acaccessreporting")]
        public string Acaccessreporting { get; set; }

        [JsonProperty("tendersreturnforeigncurrency")]
        public string Tendersreturnforeigncurrency { get; set; }

        [JsonProperty("posdiscardpendingheldtransactions")]
        public string Posdiscardpendingheldtransactions { get; set; }
    }

    public partial class Preferences
    {
        [JsonProperty("activation_expiration_date_in_days")]
        public string ActivationExpirationDateInDays { get; set; }

        [JsonProperty("apply_promotions_based_on_original_price")]
        public string ApplyPromotionsBasedOnOriginalPrice { get; set; }

        [JsonProperty("asn_vouchers_copy_received_qty_from_original_qty_on_asn")]
        public string AsnVouchersCopyReceivedQtyFromOriginalQtyOnAsn { get; set; }

        [JsonProperty("asn_vouchers_require_all_specified_packages_for_receiving")]
        public string AsnVouchersRequireAllSpecifiedPackagesForReceiving { get; set; }

        [JsonProperty("asn_vouchers_require_number_of_packages_for_receiving")]
        public string AsnVouchersRequireNumberOfPackagesForReceiving { get; set; }

        [JsonProperty("auto_lock_workstation_timer")]
        public string AutoLockWorkstationTimer { get; set; }

        [JsonProperty("auto_lock_workstation_timer_enabled")]
        public string AutoLockWorkstationTimerEnabled { get; set; }

        [JsonProperty("automatically_print_update_balanced_document")]
        public string AutomaticallyPrintUpdateBalancedDocument { get; set; }

        [JsonProperty("availability_check_voucher")]
        public string AvailabilityCheckVoucher { get; set; }

        [JsonProperty("centrals_credit_enabled")]
        public string CentralsCreditEnabled { get; set; }

        [JsonProperty("centrals_credit_negative_bal_permitted")]
        public string CentralsCreditNegativeBalPermitted { get; set; }

        [JsonProperty("centrals_credit_offline_limit")]
        public string CentralsCreditOfflineLimit { get; set; }

        [JsonProperty("centrals_credit_promt_for_cash")]
        public string CentralsCreditPromtForCash { get; set; }

        [JsonProperty("centrals_credit_promt_for_cash_less_then")]
        public string CentralsCreditPromtForCashLessThen { get; set; }

        [JsonProperty("centrals_credit_purchase_limit")]
        public string CentralsCreditPurchaseLimit { get; set; }

        [JsonProperty("centrals_credit_restrict_change_to")]
        public string CentralsCreditRestrictChangeTo { get; set; }

        [JsonProperty("centrals_credit_suggest_as_tender")]
        public string CentralsCreditSuggestAsTender { get; set; }

        [JsonProperty("centrals_customer_enabled")]
        public string CentralsCustomerEnabled { get; set; }

        [JsonProperty("centrals_customer_max_result_set")]
        public string CentralsCustomerMaxResultSet { get; set; }

        [JsonProperty("centrals_gift_card_manage_ids_by")]
        public string CentralsGiftCardManageIdsBy { get; set; }

        [JsonProperty("centrals_gift_card_mask_card_number")]
        public string CentralsGiftCardMaskCardNumber { get; set; }

        [JsonProperty("centrals_gift_cards_allow_manual_entry")]
        public string CentralsGiftCardsAllowManualEntry { get; set; }

        [JsonProperty("centrals_gift_cards_beginning_sentinel")]
        public object CentralsGiftCardsBeginningSentinel { get; set; }

        [JsonProperty("centrals_gift_cards_card_no_element")]
        public string CentralsGiftCardsCardNoElement { get; set; }

        [JsonProperty("centrals_gift_cards_card_no_length")]
        public string CentralsGiftCardsCardNoLength { get; set; }

        [JsonProperty("centrals_gift_cards_card_no_track")]
        public string CentralsGiftCardsCardNoTrack { get; set; }

        [JsonProperty("centrals_gift_cards_card_no_visible")]
        public string CentralsGiftCardsCardNoVisible { get; set; }

        [JsonProperty("centrals_gift_cards_element_seperator")]
        public object CentralsGiftCardsElementSeperator { get; set; }

        [JsonProperty("centrals_gift_cards_enabled")]
        public string CentralsGiftCardsEnabled { get; set; }

        [JsonProperty("centrals_gift_cards_generate_central_id")]
        public string CentralsGiftCardsGenerateCentralId { get; set; }

        [JsonProperty("centrals_gift_cards_negative_bal_permitted")]
        public string CentralsGiftCardsNegativeBalPermitted { get; set; }

        [JsonProperty("centrals_gift_cards_offline_limit")]
        public string CentralsGiftCardsOfflineLimit { get; set; }

        [JsonProperty("centrals_gift_cards_promt_for_cash")]
        public string CentralsGiftCardsPromtForCash { get; set; }

        [JsonProperty("centrals_gift_cards_promt_for_cash_less_then")]
        public string CentralsGiftCardsPromtForCashLessThen { get; set; }

        [JsonProperty("centrals_gift_cards_purchase_limit")]
        public string CentralsGiftCardsPurchaseLimit { get; set; }

        [JsonProperty("centrals_gift_cards_restrict_change_to")]
        public string CentralsGiftCardsRestrictChangeTo { get; set; }

        [JsonProperty("centrals_gift_cards_track_begin")]
        public object CentralsGiftCardsTrackBegin { get; set; }

        [JsonProperty("centrals_gift_cards_track_end")]
        public object CentralsGiftCardsTrackEnd { get; set; }

        [JsonProperty("centrals_returns_enabled")]
        public string CentralsReturnsEnabled { get; set; }

        [JsonProperty("centrals_returns_enforce_orig_price_tax")]
        public string CentralsReturnsEnforceOrigPriceTax { get; set; }

        [JsonProperty("centrals_returns_max_result_set")]
        public string CentralsReturnsMaxResultSet { get; set; }

        [JsonProperty("client_view_path")]
        public string ClientViewPath { get; set; }

        [JsonProperty("combine_mixed_documents_when_printing")]
        public string CombineMixedDocumentsWhenPrinting { get; set; }

        [JsonProperty("customer_general_allow_duplicate_cust_ids")]
        public string CustomerGeneralAllowDuplicateCustIds { get; set; }

        [JsonProperty("customer_general_allow_duplicate_cust_names")]
        public string CustomerGeneralAllowDuplicateCustNames { get; set; }

        [JsonProperty("customer_general_append_installation_id_to_custid")]
        public string CustomerGeneralAppendInstallationIdToCustid { get; set; }

        [JsonProperty("customer_general_cust_lookup_by")]
        public string CustomerGeneralCustLookupBy { get; set; }

        [JsonProperty("customer_general_customer_share_type")]
        public string CustomerGeneralCustomerShareType { get; set; }

        [JsonProperty("customer_general_default_cust_lookup_by")]
        public string CustomerGeneralDefaultCustLookupBy { get; set; }

        [JsonProperty("customer_general_new_cust_required_fields")]
        public string CustomerGeneralNewCustRequiredFields { get; set; }

        [JsonProperty("customer_general_zeroout_taxperc_for_expcust")]
        public string CustomerGeneralZerooutTaxpercForExpcust { get; set; }

        [JsonProperty("customer_history_prism_source")]
        public string CustomerHistoryPrismSource { get; set; }

        [JsonProperty("customer_history_v9_database")]
        public string CustomerHistoryV9Database { get; set; }

        [JsonProperty("customer_locale_address_location_type")]
        public string CustomerLocaleAddressLocationType { get; set; }

        [JsonProperty("customer_orders_allow_record_sale")]
        public string CustomerOrdersAllowRecordSale { get; set; }

        [JsonProperty("customer_security_check_limit")]
        public string CustomerSecurityCheckLimit { get; set; }

        [JsonProperty("customer_security_cust_security_level_sid")]
        public string CustomerSecurityCustSecurityLevelSid { get; set; }

        [JsonProperty("customer_security_cust_security_levels")]
        public string CustomerSecurityCustSecurityLevels { get; set; }

        [JsonProperty("customer_security_max_discpercent")]
        public string CustomerSecurityMaxDiscpercent { get; set; }

        [JsonProperty("default_eft_sid")]
        public object DefaultEftSid { get; set; }

        [JsonProperty("default_hardware_sid")]
        public object DefaultHardwareSid { get; set; }

        [JsonProperty("default_language")]
        public string DefaultLanguage { get; set; }

        [JsonProperty("default_po_type")]
        public string DefaultPoType { get; set; }

        [JsonProperty("default_price_level")]
        public string DefaultPriceLevel { get; set; }

        [JsonProperty("default_price_level_name")]
        public string DefaultPriceLevelName { get; set; }

        [JsonProperty("default_price_level_sid")]
        public string DefaultPriceLevelSid { get; set; }

        [JsonProperty("default_store_name")]
        public string DefaultStoreName { get; set; }

        [JsonProperty("default_store_number")]
        public string DefaultStoreNumber { get; set; }

        [JsonProperty("default_store_sid")]
        public string DefaultStoreSid { get; set; }

        [JsonProperty("default_sub_wks_create")]
        public string DefaultSubWksCreate { get; set; }

        [JsonProperty("default_subject_line")]
        public string DefaultSubjectLine { get; set; }

        [JsonProperty("default_sublocation_for_adjustment_memo")]
        public string DefaultSublocationForAdjustmentMemo { get; set; }

        [JsonProperty("default_sublocation_for_asn")]
        public string DefaultSublocationForAsn { get; set; }

        [JsonProperty("default_sublocation_for_pos_transaction")]
        public string DefaultSublocationForPosTransaction { get; set; }

        [JsonProperty("default_sublocation_for_slip")]
        public string DefaultSublocationForSlip { get; set; }

        [JsonProperty("default_sublocation_for_voucher")]
        public string DefaultSublocationForVoucher { get; set; }

        [JsonProperty("default_subsidiary_number")]
        public string DefaultSubsidiaryNumber { get; set; }

        [JsonProperty("default_subsidiary_sid")]
        public string DefaultSubsidiarySid { get; set; }

        [JsonProperty("default_tax_area_name")]
        public object DefaultTaxAreaName { get; set; }

        [JsonProperty("default_tax_area_sid")]
        public object DefaultTaxAreaSid { get; set; }

        [JsonProperty("default_tax_area2_name")]
        public object DefaultTaxArea2Name { get; set; }

        [JsonProperty("default_tax_area2_sid")]
        public object DefaultTaxArea2Sid { get; set; }

        [JsonProperty("disbursement_req_cash_drop")]
        public string DisbursementReqCashDrop { get; set; }

        [JsonProperty("disbursement_req_drawer_open")]
        public string DisbursementReqDrawerOpen { get; set; }

        [JsonProperty("disbursement_req_paid_in")]
        public string DisbursementReqPaidIn { get; set; }

        [JsonProperty("disbursement_req_paid_out")]
        public string DisbursementReqPaidOut { get; set; }

        [JsonProperty("document_print_package_items")]
        public string DocumentPrintPackageItems { get; set; }

        [JsonProperty("document_subject_line")]
        public string DocumentSubjectLine { get; set; }

        [JsonProperty("documents_customer_lookup_by")]
        public string DocumentsCustomerLookupBy { get; set; }

        [JsonProperty("documents_general_allow_only_one_item_type_per_document")]
        public string DocumentsGeneralAllowOnlyOneItemTypePerDocument { get; set; }

        [JsonProperty("documents_general_consolidate_items_on_docs")]
        public string DocumentsGeneralConsolidateItemsOnDocs { get; set; }

        [JsonProperty("documents_general_item_lookup_by")]
        public string DocumentsGeneralItemLookupBy { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_1")]
        public object DocumentsGeneralItemNoteMapAssignedTo1 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_10")]
        public object DocumentsGeneralItemNoteMapAssignedTo10 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_2")]
        public object DocumentsGeneralItemNoteMapAssignedTo2 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_3")]
        public object DocumentsGeneralItemNoteMapAssignedTo3 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_4")]
        public object DocumentsGeneralItemNoteMapAssignedTo4 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_5")]
        public object DocumentsGeneralItemNoteMapAssignedTo5 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_6")]
        public object DocumentsGeneralItemNoteMapAssignedTo6 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_7")]
        public object DocumentsGeneralItemNoteMapAssignedTo7 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_8")]
        public object DocumentsGeneralItemNoteMapAssignedTo8 { get; set; }

        [JsonProperty("documents_general_item_note_map_assigned_to_9")]
        public object DocumentsGeneralItemNoteMapAssignedTo9 { get; set; }

        [JsonProperty("documents_general_mask_values")]
        public object DocumentsGeneralMaskValues { get; set; }

        [JsonProperty("documents_general_pending_transaction_lookup_default_filter")]
        public string DocumentsGeneralPendingTransactionLookupDefaultFilter { get; set; }

        [JsonProperty("documents_general_print_zero_quantity_items")]
        public string DocumentsGeneralPrintZeroQuantityItems { get; set; }

        [JsonProperty("documents_general_seq_level_adjustment")]
        public string DocumentsGeneralSeqLevelAdjustment { get; set; }

        [JsonProperty("documents_general_seq_level_asn")]
        public string DocumentsGeneralSeqLevelAsn { get; set; }

        [JsonProperty("documents_general_seq_level_customer_id")]
        public string DocumentsGeneralSeqLevelCustomerId { get; set; }

        [JsonProperty("documents_general_seq_level_customer_orders")]
        public string DocumentsGeneralSeqLevelCustomerOrders { get; set; }

        [JsonProperty("documents_general_seq_level_disbursement")]
        public string DocumentsGeneralSeqLevelDisbursement { get; set; }

        [JsonProperty("documents_general_seq_level_layaway")]
        public string DocumentsGeneralSeqLevelLayaway { get; set; }

        [JsonProperty("documents_general_seq_level_order")]
        public string DocumentsGeneralSeqLevelOrder { get; set; }

        [JsonProperty("documents_general_seq_level_po")]
        public string DocumentsGeneralSeqLevelPo { get; set; }

        [JsonProperty("documents_general_seq_level_return")]
        public string DocumentsGeneralSeqLevelReturn { get; set; }

        [JsonProperty("documents_general_seq_level_sale")]
        public string DocumentsGeneralSeqLevelSale { get; set; }

        [JsonProperty("documents_general_seq_level_send_sale")]
        public string DocumentsGeneralSeqLevelSendSale { get; set; }

        [JsonProperty("documents_general_seq_level_to")]
        public string DocumentsGeneralSeqLevelTo { get; set; }

        [JsonProperty("documents_general_seq_level_transferslip")]
        public string DocumentsGeneralSeqLevelTransferslip { get; set; }

        [JsonProperty("documents_general_seq_level_voucher")]
        public string DocumentsGeneralSeqLevelVoucher { get; set; }

        [JsonProperty("documents_general_seq_level_zout")]
        public string DocumentsGeneralSeqLevelZout { get; set; }

        [JsonProperty("documents_general_use_mask_for_cost")]
        public string DocumentsGeneralUseMaskForCost { get; set; }

        [JsonProperty("documents_general_use_mask_for_last_rcvd_date")]
        public string DocumentsGeneralUseMaskForLastRcvdDate { get; set; }

        [JsonProperty("documents_general_use_mask_for_price")]
        public string DocumentsGeneralUseMaskForPrice { get; set; }

        [JsonProperty("documents_general_workstation_default_item_type")]
        public string DocumentsGeneralWorkstationDefaultItemType { get; set; }

        [JsonProperty("documents_reason_comments_doc_comments_required_checkinout")]
        public string DocumentsReasonCommentsDocCommentsRequiredCheckinout { get; set; }

        [JsonProperty("documents_reason_comments_doc_comments_required_cust_store_credit")]
        public string DocumentsReasonCommentsDocCommentsRequiredCustStoreCredit { get; set; }

        [JsonProperty("documents_reason_comments_doc_comments_required_sales")]
        public string DocumentsReasonCommentsDocCommentsRequiredSales { get; set; }

        [JsonProperty("documents_reason_comments_doc_comments_required_so")]
        public string DocumentsReasonCommentsDocCommentsRequiredSo { get; set; }

        [JsonProperty("documents_reason_comments_doc_comments_required_vouchers")]
        public string DocumentsReasonCommentsDocCommentsRequiredVouchers { get; set; }

        [JsonProperty("documents_reason_comments_doc_note_required_vouchers")]
        public string DocumentsReasonCommentsDocNoteRequiredVouchers { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_adjustments")]
        public string DocumentsReasonCommentsDocReasonRequiredAdjustments { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_asn")]
        public string DocumentsReasonCommentsDocReasonRequiredAsn { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_cash_drop")]
        public string DocumentsReasonCommentsDocReasonRequiredCashDrop { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_disbursements")]
        public string DocumentsReasonCommentsDocReasonRequiredDisbursements { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_discount")]
        public string DocumentsReasonCommentsDocReasonRequiredDiscount { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_drawer_open")]
        public string DocumentsReasonCommentsDocReasonRequiredDrawerOpen { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_paid_in")]
        public string DocumentsReasonCommentsDocReasonRequiredPaidIn { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_paid_out")]
        public string DocumentsReasonCommentsDocReasonRequiredPaidOut { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_po")]
        public string DocumentsReasonCommentsDocReasonRequiredPo { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_return")]
        public string DocumentsReasonCommentsDocReasonRequiredReturn { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_sales")]
        public string DocumentsReasonCommentsDocReasonRequiredSales { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_slips")]
        public string DocumentsReasonCommentsDocReasonRequiredSlips { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_to")]
        public string DocumentsReasonCommentsDocReasonRequiredTo { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_void")]
        public string DocumentsReasonCommentsDocReasonRequiredVoid { get; set; }

        [JsonProperty("documents_reason_comments_doc_reason_required_vouchers")]
        public string DocumentsReasonCommentsDocReasonRequiredVouchers { get; set; }

        [JsonProperty("documents_sendsale_tax_location")]
        public string DocumentsSendsaleTaxLocation { get; set; }

        [JsonProperty("eft_mw_credit_endpoint")]
        public Uri EftMwCreditEndpoint { get; set; }

        [JsonProperty("eft_mw_credit_schema")]
        public Uri EftMwCreditSchema { get; set; }

        [JsonProperty("eft_mw_display_colors")]
        public object EftMwDisplayColors { get; set; }

        [JsonProperty("eft_mw_display_options")]
        public object EftMwDisplayOptions { get; set; }

        [JsonProperty("eft_mw_gift_endpoint")]
        public Uri EftMwGiftEndpoint { get; set; }

        [JsonProperty("eft_mw_gift_schema")]
        public Uri EftMwGiftSchema { get; set; }

        [JsonProperty("eft_mw_logo_location")]
        public object EftMwLogoLocation { get; set; }

        [JsonProperty("eft_mw_merchant_key")]
        public object EftMwMerchantKey { get; set; }

        [JsonProperty("eft_mw_merchant_name")]
        public object EftMwMerchantName { get; set; }

        [JsonProperty("eft_mw_merchant_site_id")]
        public object EftMwMerchantSiteId { get; set; }

        [JsonProperty("eft_mw_redirect_location")]
        public object EftMwRedirectLocation { get; set; }

        [JsonProperty("eft_mw_requires_sig_cap")]
        public string EftMwRequiresSigCap { get; set; }

        [JsonProperty("eft_mw_sig_cap_floor_limit")]
        public string EftMwSigCapFloorLimit { get; set; }

        [JsonProperty("eft_mw_software_name")]
        public object EftMwSoftwareName { get; set; }

        [JsonProperty("eft_mw_software_version")]
        public object EftMwSoftwareVersion { get; set; }

        [JsonProperty("eft_mw_terminal_id")]
        public object EftMwTerminalId { get; set; }

        [JsonProperty("eft_mw_use_genius_device")]
        public string EftMwUseGeniusDevice { get; set; }

        [JsonProperty("eft_mw_use_genius_mini_device")]
        public string EftMwUseGeniusMiniDevice { get; set; }

        [JsonProperty("eft_mw_web_transaction_server")]
        public Uri EftMwWebTransactionServer { get; set; }

        [JsonProperty("eft_provider")]
        public string EftProvider { get; set; }

        [JsonProperty("email_server_format_type")]
        public string EmailServerFormatType { get; set; }

        [JsonProperty("email_server_host_name")]
        public object EmailServerHostName { get; set; }

        [JsonProperty("email_server_password")]
        public object EmailServerPassword { get; set; }

        [JsonProperty("email_server_password_enc")]
        public object EmailServerPasswordEnc { get; set; }

        [JsonProperty("email_server_sender")]
        public object EmailServerSender { get; set; }

        [JsonProperty("email_server_smtp_port")]
        public string EmailServerSmtpPort { get; set; }

        [JsonProperty("email_server_use_ssl")]
        public string EmailServerUseSsl { get; set; }

        [JsonProperty("email_server_user_name")]
        public object EmailServerUserName { get; set; }

        [JsonProperty("employee_general_default_associate_for_new_transactions")]
        public string EmployeeGeneralDefaultAssociateForNewTransactions { get; set; }

        [JsonProperty("employee_general_enable_new_employee_as_cust")]
        public string EmployeeGeneralEnableNewEmployeeAsCust { get; set; }

        [JsonProperty("employee_general_generate_high_security_receipts_for_adjustments")]
        public string EmployeeGeneralGenerateHighSecurityReceiptsForAdjustments { get; set; }

        [JsonProperty("employee_general_generate_high_security_receipts_for_pos")]
        public string EmployeeGeneralGenerateHighSecurityReceiptsForPos { get; set; }

        [JsonProperty("employee_general_generate_high_security_receipts_for_transferslips")]
        public string EmployeeGeneralGenerateHighSecurityReceiptsForTransferslips { get; set; }

        [JsonProperty("employee_general_generate_high_security_receipts_for_vouchers")]
        public string EmployeeGeneralGenerateHighSecurityReceiptsForVouchers { get; set; }

        [JsonProperty("employee_general_new_emp_required_fields")]
        public string EmployeeGeneralNewEmpRequiredFields { get; set; }

        [JsonProperty("employee_general_require_comment")]
        public string EmployeeGeneralRequireComment { get; set; }

        [JsonProperty("employee_general_require_user_to_select_associate_on_new_transactions")]
        public bool EmployeeGeneralRequireUserToSelectAssociateOnNewTransactions { get; set; }

        [JsonProperty("employee_general_show_associate_as")]
        public string EmployeeGeneralShowAssociateAs { get; set; }

        [JsonProperty("enable_intercompany_transfers")]
        public string EnableIntercompanyTransfers { get; set; }

        [JsonProperty("enable_sublocations")]
        public string EnableSublocations { get; set; }

        [JsonProperty("enable_touch_screen_select_inputs")]
        public string EnableTouchScreenSelectInputs { get; set; }

        [JsonProperty("enforce_password_history")]
        public string EnforcePasswordHistory { get; set; }

        [JsonProperty("enforce_strong_password")]
        public string EnforceStrongPassword { get; set; }

        [JsonProperty("error_toast_timeout")]
        public string ErrorToastTimeout { get; set; }

        [JsonProperty("global_locality_country_code")]
        public string GlobalLocalityCountryCode { get; set; }

        [JsonProperty("global_locality_country_name")]
        public string GlobalLocalityCountryName { get; set; }

        [JsonProperty("global_locality_language")]
        public string GlobalLocalityLanguage { get; set; }

        [JsonProperty("grid_data")]
        public string GridData { get; set; }

        [JsonProperty("hide_default_print_designs")]
        public string HideDefaultPrintDesigns { get; set; }

        [JsonProperty("hsr__exit_document")]
        public string HsrExitDocument { get; set; }

        [JsonProperty("hsr__open_drawer")]
        public string HsrOpenDrawer { get; set; }

        [JsonProperty("hsr__reboot")]
        public string HsrReboot { get; set; }

        [JsonProperty("image_display_type")]
        public string ImageDisplayType { get; set; }

        [JsonProperty("image_save_directory")]
        public string ImageSaveDirectory { get; set; }

        [JsonProperty("image_server")]
        public object ImageServer { get; set; }

        [JsonProperty("intercompany_ts_generate_doc_upon_update")]
        public string IntercompanyTsGenerateDocUponUpdate { get; set; }

        [JsonProperty("inventory_alert_when_item_price_differs_with_style_price")]
        public string InventoryAlertWhenItemPriceDiffersWithStylePrice { get; set; }

        [JsonProperty("inventory_allow_duplicate_alu")]
        public bool InventoryAllowDuplicateAlu { get; set; }

        [JsonProperty("inventory_allow_duplicate_upc")]
        public string InventoryAllowDuplicateUpc { get; set; }

        [JsonProperty("inventory_allow_negative_quantities")]
        public string InventoryAllowNegativeQuantities { get; set; }

        [JsonProperty("inventory_cost_difference_threshold")]
        public string InventoryCostDifferenceThreshold { get; set; }

        [JsonProperty("inventory_costing_method")]
        public string InventoryCostingMethod { get; set; }

        [JsonProperty("inventory_default_columns")]
        public string InventoryDefaultColumns { get; set; }

        [JsonProperty("inventory_default_commission_code_sid")]
        public object InventoryDefaultCommissionCodeSid { get; set; }

        [JsonProperty("inventory_default_filters")]
        public string InventoryDefaultFilters { get; set; }

        [JsonProperty("inventory_default_maximum_discount")]
        public string InventoryDefaultMaximumDiscount { get; set; }

        [JsonProperty("inventory_discount_price_level_sid")]
        public object InventoryDiscountPriceLevelSid { get; set; }

        [JsonProperty("inventory_enable_alu_sequence")]
        public bool InventoryEnableAluSequence { get; set; }

        [JsonProperty("inventory_enable_cost_difference_threshold")]
        public string InventoryEnableCostDifferenceThreshold { get; set; }

        [JsonProperty("inventory_enable_style_udfs")]
        public string InventoryEnableStyleUdfs { get; set; }

        [JsonProperty("inventory_enable_upc_sequence")]
        public string InventoryEnableUpcSequence { get; set; }

        [JsonProperty("inventory_general_seq_level_alu")]
        public string InventoryGeneralSeqLevelAlu { get; set; }

        [JsonProperty("inventory_general_seq_level_inventory_media")]
        public string InventoryGeneralSeqLevelInventoryMedia { get; set; }

        [JsonProperty("inventory_general_seq_level_upc")]
        public string InventoryGeneralSeqLevelUpc { get; set; }

        [JsonProperty("inventory_have_margins_affect_cost")]
        public string InventoryHaveMarginsAffectCost { get; set; }

        [JsonProperty("inventory_style_definition_field")]
        public string InventoryStyleDefinitionField { get; set; }

        [JsonProperty("layaway_orders_allow_record_sale")]
        public string LayawayOrdersAllowRecordSale { get; set; }

        [JsonProperty("lock_after_failed_logon_attempts")]
        public string LockAfterFailedLogonAttempts { get; set; }

        [JsonProperty("lockout_duration")]
        public string LockoutDuration { get; set; }

        [JsonProperty("log_failed_login")]
        public string LogFailedLogin { get; set; }

        [JsonProperty("log_password_change")]
        public string LogPasswordChange { get; set; }

        [JsonProperty("log_successfull_login")]
        public string LogSuccessfullLogin { get; set; }

        [JsonProperty("log_user_group_change")]
        public string LogUserGroupChange { get; set; }

        [JsonProperty("markdown_default_update_tiem")]
        public string MarkdownDefaultUpdateTiem { get; set; }

        [JsonProperty("merchandise_adjustments_after_memo_update_goto")]
        public string MerchandiseAdjustmentsAfterMemoUpdateGoto { get; set; }

        [JsonProperty("merchandise_adjustments_allow_negative_qty_on_qty_memo")]
        public string MerchandiseAdjustmentsAllowNegativeQtyOnQtyMemo { get; set; }

        [JsonProperty("merchandise_adjustments_create_price_memo_for_zerooh_qty")]
        public string MerchandiseAdjustmentsCreatePriceMemoForZeroohQty { get; set; }

        [JsonProperty("merchandise_adjustments_default_cost_adj_reason")]
        public object MerchandiseAdjustmentsDefaultCostAdjReason { get; set; }

        [JsonProperty("merchandise_adjustments_default_price_adj_reason")]
        public object MerchandiseAdjustmentsDefaultPriceAdjReason { get; set; }

        [JsonProperty("merchandise_adjustments_default_qty_adj_reason")]
        public object MerchandiseAdjustmentsDefaultQtyAdjReason { get; set; }

        [JsonProperty("merchandise_adjustments_require_comment_on_memos")]
        public string MerchandiseAdjustmentsRequireCommentOnMemos { get; set; }

        [JsonProperty("merchandise_adjustments_require_cost_adj_reason")]
        public string MerchandiseAdjustmentsRequireCostAdjReason { get; set; }

        [JsonProperty("merchandise_adjustments_require_price_adj_reason")]
        public string MerchandiseAdjustmentsRequirePriceAdjReason { get; set; }

        [JsonProperty("merchandise_adjustments_require_qty_adj_reason")]
        public string MerchandiseAdjustmentsRequireQtyAdjReason { get; set; }

        [JsonProperty("merchandise_adjustments_save_store_qty_on_memos")]
        public string MerchandiseAdjustmentsSaveStoreQtyOnMemos { get; set; }

        [JsonProperty("merchandise_adjustments_use_tax_area1_to_calc_pwt_on_memos")]
        public string MerchandiseAdjustmentsUseTaxArea1ToCalcPwtOnMemos { get; set; }

        [JsonProperty("merchandise_adjustments_use_tax_area2_to_calc_pwt_on_memos")]
        public string MerchandiseAdjustmentsUseTaxArea2ToCalcPwtOnMemos { get; set; }

        [JsonProperty("merchandise_general_item_sid_generation_method")]
        public string MerchandiseGeneralItemSidGenerationMethod { get; set; }

        [JsonProperty("merchandise_general_style_sid_generation_method")]
        public string MerchandiseGeneralStyleSidGenerationMethod { get; set; }

        [JsonProperty("merchandise_general_use_first_n_characters_for_style_sid")]
        public string MerchandiseGeneralUseFirstNCharactersForStyleSid { get; set; }

        [JsonProperty("merchandise_pi_allow_updatepi_with_missing_serial_lot_numbers")]
        public string MerchandisePiAllowUpdatepiWithMissingSerialLotNumbers { get; set; }

        [JsonProperty("merchandise_pi_prompt_serial_lot_when_add_edit_picount")]
        public string MerchandisePiPromptSerialLotWhenAddEditPicount { get; set; }

        [JsonProperty("merchandise_pi_use_pagination_count")]
        public string MerchandisePiUsePaginationCount { get; set; }

        [JsonProperty("merchandise_pricing_disc_price_level")]
        public string MerchandisePricingDiscPriceLevel { get; set; }

        [JsonProperty("merchandise_pricing_have_margin_effect_price_cost")]
        public string MerchandisePricingHaveMarginEffectPriceCost { get; set; }

        [JsonProperty("merchandise_pricing_max_accum_disc_allowed_fornewitems")]
        public string MerchandisePricingMaxAccumDiscAllowedFornewitems { get; set; }

        [JsonProperty("merchandise_pricing_max_disc_allowed_for_newitems")]
        public string MerchandisePricingMaxDiscAllowedForNewitems { get; set; }

        [JsonProperty("merchandise_pricing_use_seasonal_pricing")]
        public string MerchandisePricingUseSeasonalPricing { get; set; }

        [JsonProperty("merchandise_scale_scale_order_display")]
        public string MerchandiseScaleScaleOrderDisplay { get; set; }

        [JsonProperty("number_of_segments")]
        public string NumberOfSegments { get; set; }

        [JsonProperty("open_cash_drawer_prevents_new_receipt")]
        public string OpenCashDrawerPreventsNewReceipt { get; set; }

        [JsonProperty("open_drawer_after_close")]
        public string OpenDrawerAfterClose { get; set; }

        [JsonProperty("order_due_days")]
        public string OrderDueDays { get; set; }

        [JsonProperty("password_expires_after")]
        public string PasswordExpiresAfter { get; set; }

        [JsonProperty("password_minimum_length")]
        public string PasswordMinimumLength { get; set; }

        [JsonProperty("password_requires_number")]
        public string PasswordRequiresNumber { get; set; }

        [JsonProperty("password_requires_special_characters")]
        public string PasswordRequiresSpecialCharacters { get; set; }

        [JsonProperty("password_requires_uppercase_character")]
        public string PasswordRequiresUppercaseCharacter { get; set; }

        [JsonProperty("peripherals_output_deniedreceipts_action")]
        public string PeripheralsOutputDeniedreceiptsAction { get; set; }

        [JsonProperty("peripherals_output_deniedreceipts_email_design")]
        public string PeripheralsOutputDeniedreceiptsEmailDesign { get; set; }

        [JsonProperty("peripherals_output_deniedreceipts_email_subject")]
        public string PeripheralsOutputDeniedreceiptsEmailSubject { get; set; }

        [JsonProperty("peripherals_output_deniedreceipts_preview_design")]
        public string PeripheralsOutputDeniedreceiptsPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_deniedreceipts_print_design")]
        public string PeripheralsOutputDeniedreceiptsPrintDesign { get; set; }

        [JsonProperty("peripherals_output_deniedreceipts_print_printer")]
        public object PeripheralsOutputDeniedreceiptsPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_documents_action")]
        public string PeripheralsOutputDocumentsAction { get; set; }

        [JsonProperty("peripherals_output_documents_email_design")]
        public string PeripheralsOutputDocumentsEmailDesign { get; set; }

        [JsonProperty("peripherals_output_documents_email_subject")]
        public string PeripheralsOutputDocumentsEmailSubject { get; set; }

        [JsonProperty("peripherals_output_documents_preview_design")]
        public string PeripheralsOutputDocumentsPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_documents_print_design")]
        public string PeripheralsOutputDocumentsPrintDesign { get; set; }

        [JsonProperty("peripherals_output_documents_print_printer")]
        public object PeripheralsOutputDocumentsPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_drawerevent_action")]
        public string PeripheralsOutputDrawereventAction { get; set; }

        [JsonProperty("peripherals_output_drawerevent_email_design")]
        public string PeripheralsOutputDrawereventEmailDesign { get; set; }

        [JsonProperty("peripherals_output_drawerevent_email_subject")]
        public string PeripheralsOutputDrawereventEmailSubject { get; set; }

        [JsonProperty("peripherals_output_drawerevent_preview_design")]
        public string PeripheralsOutputDrawereventPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_drawerevent_print_design")]
        public string PeripheralsOutputDrawereventPrintDesign { get; set; }

        [JsonProperty("peripherals_output_drawerevent_print_printer")]
        public object PeripheralsOutputDrawereventPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_giftcardbalance_action")]
        public string PeripheralsOutputGiftcardbalanceAction { get; set; }

        [JsonProperty("peripherals_output_giftcardbalance_email_design")]
        public string PeripheralsOutputGiftcardbalanceEmailDesign { get; set; }

        [JsonProperty("peripherals_output_giftcardbalance_email_subject")]
        public string PeripheralsOutputGiftcardbalanceEmailSubject { get; set; }

        [JsonProperty("peripherals_output_giftcardbalance_preview_design")]
        public string PeripheralsOutputGiftcardbalancePreviewDesign { get; set; }

        [JsonProperty("peripherals_output_giftcardbalance_print_design")]
        public string PeripheralsOutputGiftcardbalancePrintDesign { get; set; }

        [JsonProperty("peripherals_output_giftcardbalance_print_printer")]
        public object PeripheralsOutputGiftcardbalancePrintPrinter { get; set; }

        [JsonProperty("peripherals_output_invntags_action")]
        public string PeripheralsOutputInvntagsAction { get; set; }

        [JsonProperty("peripherals_output_invntags_email_design")]
        public string PeripheralsOutputInvntagsEmailDesign { get; set; }

        [JsonProperty("peripherals_output_invntags_email_subject")]
        public string PeripheralsOutputInvntagsEmailSubject { get; set; }

        [JsonProperty("peripherals_output_invntags_preview_design")]
        public string PeripheralsOutputInvntagsPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_invntags_print_design")]
        public string PeripheralsOutputInvntagsPrintDesign { get; set; }

        [JsonProperty("peripherals_output_invntags_print_printer")]
        public object PeripheralsOutputInvntagsPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_pisheet_action")]
        public string PeripheralsOutputPisheetAction { get; set; }

        [JsonProperty("peripherals_output_pisheet_email_design")]
        public string PeripheralsOutputPisheetEmailDesign { get; set; }

        [JsonProperty("peripherals_output_pisheet_email_subject")]
        public string PeripheralsOutputPisheetEmailSubject { get; set; }

        [JsonProperty("peripherals_output_pisheet_preview_design")]
        public string PeripheralsOutputPisheetPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_pisheet_print_design")]
        public string PeripheralsOutputPisheetPrintDesign { get; set; }

        [JsonProperty("peripherals_output_pisheet_print_printer")]
        public object PeripheralsOutputPisheetPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_potags_action")]
        public string PeripheralsOutputPotagsAction { get; set; }

        [JsonProperty("peripherals_output_potags_email_design")]
        public string PeripheralsOutputPotagsEmailDesign { get; set; }

        [JsonProperty("peripherals_output_potags_email_subject")]
        public string PeripheralsOutputPotagsEmailSubject { get; set; }

        [JsonProperty("peripherals_output_potags_preview_design")]
        public string PeripheralsOutputPotagsPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_potags_print_design")]
        public string PeripheralsOutputPotagsPrintDesign { get; set; }

        [JsonProperty("peripherals_output_potags_print_printer")]
        public object PeripheralsOutputPotagsPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_purchaseorder_action")]
        public string PeripheralsOutputPurchaseorderAction { get; set; }

        [JsonProperty("peripherals_output_purchaseorder_email_design")]
        public string PeripheralsOutputPurchaseorderEmailDesign { get; set; }

        [JsonProperty("peripherals_output_purchaseorder_email_subject")]
        public string PeripheralsOutputPurchaseorderEmailSubject { get; set; }

        [JsonProperty("peripherals_output_purchaseorder_preview_design")]
        public string PeripheralsOutputPurchaseorderPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_purchaseorder_print_design")]
        public string PeripheralsOutputPurchaseorderPrintDesign { get; set; }

        [JsonProperty("peripherals_output_purchaseorder_print_printer")]
        public object PeripheralsOutputPurchaseorderPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_receiving_action")]
        public string PeripheralsOutputReceivingAction { get; set; }

        [JsonProperty("peripherals_output_receiving_email_design")]
        public string PeripheralsOutputReceivingEmailDesign { get; set; }

        [JsonProperty("peripherals_output_receiving_email_subject")]
        public string PeripheralsOutputReceivingEmailSubject { get; set; }

        [JsonProperty("peripherals_output_receiving_preview_design")]
        public string PeripheralsOutputReceivingPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_receiving_print_design")]
        public string PeripheralsOutputReceivingPrintDesign { get; set; }

        [JsonProperty("peripherals_output_receiving_print_printer")]
        public object PeripheralsOutputReceivingPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_recvtags_action")]
        public string PeripheralsOutputRecvtagsAction { get; set; }

        [JsonProperty("peripherals_output_recvtags_email_design")]
        public string PeripheralsOutputRecvtagsEmailDesign { get; set; }

        [JsonProperty("peripherals_output_recvtags_email_subject")]
        public string PeripheralsOutputRecvtagsEmailSubject { get; set; }

        [JsonProperty("peripherals_output_recvtags_preview_design")]
        public string PeripheralsOutputRecvtagsPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_recvtags_print_design")]
        public string PeripheralsOutputRecvtagsPrintDesign { get; set; }

        [JsonProperty("peripherals_output_recvtags_print_printer")]
        public object PeripheralsOutputRecvtagsPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_sliptags_action")]
        public string PeripheralsOutputSliptagsAction { get; set; }

        [JsonProperty("peripherals_output_sliptags_email_design")]
        public string PeripheralsOutputSliptagsEmailDesign { get; set; }

        [JsonProperty("peripherals_output_sliptags_email_subject")]
        public string PeripheralsOutputSliptagsEmailSubject { get; set; }

        [JsonProperty("peripherals_output_sliptags_preview_design")]
        public string PeripheralsOutputSliptagsPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_sliptags_print_design")]
        public string PeripheralsOutputSliptagsPrintDesign { get; set; }

        [JsonProperty("peripherals_output_sliptags_print_printer")]
        public object PeripheralsOutputSliptagsPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_transferslip_action")]
        public string PeripheralsOutputTransferslipAction { get; set; }

        [JsonProperty("peripherals_output_transferslip_email_design")]
        public string PeripheralsOutputTransferslipEmailDesign { get; set; }

        [JsonProperty("peripherals_output_transferslip_email_subject")]
        public string PeripheralsOutputTransferslipEmailSubject { get; set; }

        [JsonProperty("peripherals_output_transferslip_preview_design")]
        public string PeripheralsOutputTransferslipPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_transferslip_print_design")]
        public string PeripheralsOutputTransferslipPrintDesign { get; set; }

        [JsonProperty("peripherals_output_transferslip_print_printer")]
        public object PeripheralsOutputTransferslipPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_xout_action")]
        public string PeripheralsOutputXoutAction { get; set; }

        [JsonProperty("peripherals_output_xout_email_design")]
        public string PeripheralsOutputXoutEmailDesign { get; set; }

        [JsonProperty("peripherals_output_xout_email_subject")]
        public string PeripheralsOutputXoutEmailSubject { get; set; }

        [JsonProperty("peripherals_output_xout_preview_design")]
        public string PeripheralsOutputXoutPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_xout_print_design")]
        public string PeripheralsOutputXoutPrintDesign { get; set; }

        [JsonProperty("peripherals_output_xout_print_printer")]
        public object PeripheralsOutputXoutPrintPrinter { get; set; }

        [JsonProperty("peripherals_output_zout_action")]
        public string PeripheralsOutputZoutAction { get; set; }

        [JsonProperty("peripherals_output_zout_email_design")]
        public string PeripheralsOutputZoutEmailDesign { get; set; }

        [JsonProperty("peripherals_output_zout_email_subject")]
        public string PeripheralsOutputZoutEmailSubject { get; set; }

        [JsonProperty("peripherals_output_zout_preview_design")]
        public string PeripheralsOutputZoutPreviewDesign { get; set; }

        [JsonProperty("peripherals_output_zout_print_design")]
        public string PeripheralsOutputZoutPrintDesign { get; set; }

        [JsonProperty("peripherals_output_zout_print_printer")]
        public object PeripheralsOutputZoutPrintPrinter { get; set; }

        [JsonProperty("physical_inventory_activate_inactive_items_during_piupdate")]
        public string PhysicalInventoryActivateInactiveItemsDuringPiupdate { get; set; }

        [JsonProperty("physical_inventory_allow_ln_discrepancies")]
        public string PhysicalInventoryAllowLnDiscrepancies { get; set; }

        [JsonProperty("physical_inventory_allow_sn_discrepancies")]
        public string PhysicalInventoryAllowSnDiscrepancies { get; set; }

        [JsonProperty("physical_inventory_enable_ln_counts")]
        public string PhysicalInventoryEnableLnCounts { get; set; }

        [JsonProperty("physical_inventory_enable_sn_counts")]
        public string PhysicalInventoryEnableSnCounts { get; set; }

        [JsonProperty("physical_inventory_send_destination")]
        public string PhysicalInventorySendDestination { get; set; }

        [JsonProperty("physical_inventory_type")]
        public string PhysicalInventoryType { get; set; }

        [JsonProperty("populate_max_discount_from")]
        public string PopulateMaxDiscountFrom { get; set; }

        [JsonProperty("pos_general_prompt_for_decimal_qty_items")]
        public string PosGeneralPromptForDecimalQtyItems { get; set; }

        [JsonProperty("pos_hardware_cash_drawer_status_enabled")]
        public string PosHardwareCashDrawerStatusEnabled { get; set; }

        [JsonProperty("pos_hardware_line_display_item_fieldname1")]
        public string PosHardwareLineDisplayItemFieldname1 { get; set; }

        [JsonProperty("pos_hardware_line_display_item_fieldname2")]
        public string PosHardwareLineDisplayItemFieldname2 { get; set; }

        [JsonProperty("pos_hardware_line_display_new_doc_line1_alignment")]
        public string PosHardwareLineDisplayNewDocLine1Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_new_doc_line1_text")]
        public object PosHardwareLineDisplayNewDocLine1Text { get; set; }

        [JsonProperty("pos_hardware_line_display_new_doc_line2_alignment")]
        public string PosHardwareLineDisplayNewDocLine2Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_new_doc_line2_text")]
        public object PosHardwareLineDisplayNewDocLine2Text { get; set; }

        [JsonProperty("pos_hardware_line_display_new_item_line1_alignment")]
        public string PosHardwareLineDisplayNewItemLine1Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_new_item_line1_text")]
        public object PosHardwareLineDisplayNewItemLine1Text { get; set; }

        [JsonProperty("pos_hardware_line_display_new_item_line2_alignment")]
        public string PosHardwareLineDisplayNewItemLine2Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_new_item_line2_text")]
        public object PosHardwareLineDisplayNewItemLine2Text { get; set; }

        [JsonProperty("pos_hardware_line_display_startup_line1_alignment")]
        public string PosHardwareLineDisplayStartupLine1Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_startup_line1_text")]
        public string PosHardwareLineDisplayStartupLine1Text { get; set; }

        [JsonProperty("pos_hardware_line_display_startup_line2_alignment")]
        public string PosHardwareLineDisplayStartupLine2Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_startup_line2_text")]
        public string PosHardwareLineDisplayStartupLine2Text { get; set; }

        [JsonProperty("pos_hardware_line_display_total_fieldname1")]
        public string PosHardwareLineDisplayTotalFieldname1 { get; set; }

        [JsonProperty("pos_hardware_line_display_total_fieldname2")]
        public string PosHardwareLineDisplayTotalFieldname2 { get; set; }

        [JsonProperty("pos_hardware_line_display_total_line1_alignment")]
        public string PosHardwareLineDisplayTotalLine1Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_total_line1_text")]
        public object PosHardwareLineDisplayTotalLine1Text { get; set; }

        [JsonProperty("pos_hardware_line_display_total_line2_alignment")]
        public string PosHardwareLineDisplayTotalLine2Alignment { get; set; }

        [JsonProperty("pos_hardware_line_display_total_line2_text")]
        public object PosHardwareLineDisplayTotalLine2Text { get; set; }

        [JsonProperty("pos_hardware_shopper_display_description")]
        public string PosHardwareShopperDisplayDescription { get; set; }

        [JsonProperty("pos_hardware_use_wedge_barcode_scanner")]
        public string PosHardwareUseWedgeBarcodeScanner { get; set; }

        [JsonProperty("pos_loyalty_charge_tax")]
        public string PosLoyaltyChargeTax { get; set; }

        [JsonProperty("pos_loyalty_default_level_sid")]
        public string PosLoyaltyDefaultLevelSid { get; set; }

        [JsonProperty("pos_loyalty_enabled")]
        public bool PosLoyaltyEnabled { get; set; }

        [JsonProperty("pos_loyalty_enforce_returning_points")]
        public string PosLoyaltyEnforceReturningPoints { get; set; }

        [JsonProperty("pos_loyalty_include_tax")]
        public string PosLoyaltyIncludeTax { get; set; }

        [JsonProperty("pos_loyalty_offline_limit")]
        public string PosLoyaltyOfflineLimit { get; set; }

        [JsonProperty("pos_loyalty_optin_mode")]
        public string PosLoyaltyOptinMode { get; set; }

        [JsonProperty("pos_loyalty_point_decimals")]
        public string PosLoyaltyPointDecimals { get; set; }

        [JsonProperty("pos_loyalty_redemption_type")]
        public string PosLoyaltyRedemptionType { get; set; }

        [JsonProperty("pos_loyalty_suggest_using_points")]
        public bool PosLoyaltySuggestUsingPoints { get; set; }

        [JsonProperty("pos_loyalty_use_enroll_date_for_points")]
        public string PosLoyaltyUseEnrollDateForPoints { get; set; }

        [JsonProperty("pos_options_fee_shipping_default_shipping_perc")]
        public string PosOptionsFeeShippingDefaultShippingPerc { get; set; }

        [JsonProperty("pos_options_fee_shipping_have_default_shipping_perc")]
        public string PosOptionsFeeShippingHaveDefaultShippingPerc { get; set; }

        [JsonProperty("pos_options_fee_shipping_include_tax_in_shipping_amt")]
        public string PosOptionsFeeShippingIncludeTaxInShippingAmt { get; set; }

        [JsonProperty("pos_options_general_accumulate_manual_discounts")]
        public string PosOptionsGeneralAccumulateManualDiscounts { get; set; }

        [JsonProperty("pos_options_general_accumulate_spread_discounts")]
        public string PosOptionsGeneralAccumulateSpreadDiscounts { get; set; }

        [JsonProperty("pos_options_general_apply_as_global_discount")]
        public string PosOptionsGeneralApplyAsGlobalDiscount { get; set; }

        [JsonProperty("pos_options_general_apply_orig_gd_to_return_items")]
        public string PosOptionsGeneralApplyOrigGdToReturnItems { get; set; }

        [JsonProperty("pos_options_general_auto_spread_global_disc_on_orders")]
        public string PosOptionsGeneralAutoSpreadGlobalDiscOnOrders { get; set; }

        [JsonProperty("pos_options_general_auto_spread_global_disc_on_receipts")]
        public string PosOptionsGeneralAutoSpreadGlobalDiscOnReceipts { get; set; }

        [JsonProperty("pos_options_general_availability_check_customer_order")]
        public string PosOptionsGeneralAvailabilityCheckCustomerOrder { get; set; }

        [JsonProperty("pos_options_general_availability_check_layaway_order")]
        public string PosOptionsGeneralAvailabilityCheckLayawayOrder { get; set; }

        [JsonProperty("pos_options_general_availability_check_sale")]
        public string PosOptionsGeneralAvailabilityCheckSale { get; set; }

        [JsonProperty("pos_options_general_availability_check_send_sale")]
        public string PosOptionsGeneralAvailabilityCheckSendSale { get; set; }

        [JsonProperty("pos_options_general_bring_orig_price_from_price_lvl")]
        public string PosOptionsGeneralBringOrigPriceFromPriceLvl { get; set; }

        [JsonProperty("pos_options_general_default_discount_type")]
        public string PosOptionsGeneralDefaultDiscountType { get; set; }

        [JsonProperty("pos_options_general_disable_display_of_negative_discount_pct")]
        public string PosOptionsGeneralDisableDisplayOfNegativeDiscountPct { get; set; }

        [JsonProperty("pos_options_general_discount_based_on")]
        public string PosOptionsGeneralDiscountBasedOn { get; set; }

        [JsonProperty("pos_options_general_discount_rounding_method")]
        public string PosOptionsGeneralDiscountRoundingMethod { get; set; }

        [JsonProperty("pos_options_general_empl_max_discount_override")]
        public string PosOptionsGeneralEmplMaxDiscountOverride { get; set; }

        [JsonProperty("pos_options_general_print_zero_qty_items_on_transactions")]
        public string PosOptionsGeneralPrintZeroQtyItemsOnTransactions { get; set; }

        [JsonProperty("pos_options_general_prompt_for_price_on_zero_price_items")]
        public string PosOptionsGeneralPromptForPriceOnZeroPriceItems { get; set; }

        [JsonProperty("pos_options_general_restrict_item_disc_not_exceed_spreadable_global_disc_perc")]
        public string PosOptionsGeneralRestrictItemDiscNotExceedSpreadableGlobalDiscPerc { get; set; }

        [JsonProperty("pos_options_general_rounding_method")]
        public string PosOptionsGeneralRoundingMethod { get; set; }

        [JsonProperty("pos_options_general_rounding_multiplier")]
        public string PosOptionsGeneralRoundingMultiplier { get; set; }

        [JsonProperty("pos_options_general_security_receipts_when_drawer_opened_manually")]
        public string PosOptionsGeneralSecurityReceiptsWhenDrawerOpenedManually { get; set; }

        [JsonProperty("pos_options_general_security_receipts_when_receipt_progress_exited")]
        public string PosOptionsGeneralSecurityReceiptsWhenReceiptProgressExited { get; set; }

        [JsonProperty("pos_options_general_show_discount_reasons_prompt")]
        public string PosOptionsGeneralShowDiscountReasonsPrompt { get; set; }

        [JsonProperty("pos_options_general_show_one_item_discount_option")]
        public string PosOptionsGeneralShowOneItemDiscountOption { get; set; }

        [JsonProperty("pos_options_general_show_one_item_discount_type")]
        public string PosOptionsGeneralShowOneItemDiscountType { get; set; }

        [JsonProperty("pos_options_general_single_item_discount_type")]
        public string PosOptionsGeneralSingleItemDiscountType { get; set; }

        [JsonProperty("pos_options_general_tender_types_used_in_rounding")]
        public string PosOptionsGeneralTenderTypesUsedInRounding { get; set; }

        [JsonProperty("pos_options_general_transaction_discount_spread_type")]
        public string PosOptionsGeneralTransactionDiscountSpreadType { get; set; }

        [JsonProperty("pos_options_general_use_cust_defined_price_lvl")]
        public string PosOptionsGeneralUseCustDefinedPriceLvl { get; set; }

        [JsonProperty("pos_options_general_use_doc_sequence_on_security_receipts")]
        public string PosOptionsGeneralUseDocSequenceOnSecurityReceipts { get; set; }

        [JsonProperty("pos_options_general_use_forward_based_qty_pricing")]
        public string PosOptionsGeneralUseForwardBasedQtyPricing { get; set; }

        [JsonProperty("pos_options_general_use_qty_pricing")]
        public string PosOptionsGeneralUseQtyPricing { get; set; }

        [JsonProperty("pos_options_lot_no_deactivate_when_item_depleted")]
        public string PosOptionsLotNoDeactivateWhenItemDepleted { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_memos")]
        public string PosOptionsLotNoPartialControlPromptMemos { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_orders")]
        public string PosOptionsLotNoPartialControlPromptOrders { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_returns")]
        public string PosOptionsLotNoPartialControlPromptReturns { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_sales")]
        public string PosOptionsLotNoPartialControlPromptSales { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_send_sale")]
        public string PosOptionsLotNoPartialControlPromptSendSale { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_transfers")]
        public string PosOptionsLotNoPartialControlPromptTransfers { get; set; }

        [JsonProperty("pos_options_lot_no_partial_control_prompt_vouchers")]
        public string PosOptionsLotNoPartialControlPromptVouchers { get; set; }

        [JsonProperty("pos_options_packages_kits_count_package_as_single_item")]
        public string PosOptionsPackagesKitsCountPackageAsSingleItem { get; set; }

        [JsonProperty("pos_options_packages_kits_print_kit_item_on_transactions")]
        public string PosOptionsPackagesKitsPrintKitItemOnTransactions { get; set; }

        [JsonProperty("pos_options_packages_kits_print_kit_item_price_on_transactions")]
        public string PosOptionsPackagesKitsPrintKitItemPriceOnTransactions { get; set; }

        [JsonProperty("pos_options_packages_kits_print_package_item_on_transactions")]
        public string PosOptionsPackagesKitsPrintPackageItemOnTransactions { get; set; }

        [JsonProperty("pos_options_packages_kits_print_package_item_price_on_transactions")]
        public string PosOptionsPackagesKitsPrintPackageItemPriceOnTransactions { get; set; }

        [JsonProperty("pos_options_packages_kits_when_calculating_tax_on_transactions")]
        public string PosOptionsPackagesKitsWhenCalculatingTaxOnTransactions { get; set; }

        [JsonProperty("pos_options_pos_flags_note_fields_require_note_on_lost_sales")]
        public string PosOptionsPosFlagsNoteFieldsRequireNoteOnLostSales { get; set; }

        [JsonProperty("pos_options_pos_flags_note_fields_require_note_on_order_items")]
        public string PosOptionsPosFlagsNoteFieldsRequireNoteOnOrderItems { get; set; }

        [JsonProperty("pos_options_pos_flags_note_fields_require_note_on_return_items")]
        public string PosOptionsPosFlagsNoteFieldsRequireNoteOnReturnItems { get; set; }

        [JsonProperty("pos_options_pos_flags_note_fields_require_note_on_sale_items")]
        public string PosOptionsPosFlagsNoteFieldsRequireNoteOnSaleItems { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_memos")]
        public string PosOptionsSerNoPartialControlPromptMemos { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_orders")]
        public string PosOptionsSerNoPartialControlPromptOrders { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_returns")]
        public string PosOptionsSerNoPartialControlPromptReturns { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_sales")]
        public string PosOptionsSerNoPartialControlPromptSales { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_send_sale")]
        public string PosOptionsSerNoPartialControlPromptSendSale { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_transfers")]
        public string PosOptionsSerNoPartialControlPromptTransfers { get; set; }

        [JsonProperty("pos_options_ser_no_partial_control_prompt_vouchers")]
        public string PosOptionsSerNoPartialControlPromptVouchers { get; set; }

        [JsonProperty("pos_orders_shipping_method")]
        public object PosOrdersShippingMethod { get; set; }

        [JsonProperty("pos_tenders_accepted_currencies_give")]
        public object PosTendersAcceptedCurrenciesGive { get; set; }

        [JsonProperty("pos_tenders_accepted_currencies_take")]
        public object PosTendersAcceptedCurrenciesTake { get; set; }

        [JsonProperty("pos_tenders_checks_allow_cashback")]
        public string PosTendersChecksAllowCashback { get; set; }

        [JsonProperty("pos_tenders_checks_allow_eft_receipts_to_print_when_not_in_use")]
        public string PosTendersChecksAllowEftReceiptsToPrintWhenNotInUse { get; set; }

        [JsonProperty("pos_tenders_checks_max_cashback_amt")]
        public string PosTendersChecksMaxCashbackAmt { get; set; }

        [JsonProperty("pos_tenders_credit_card_auth_code_required")]
        public string PosTendersCreditCardAuthCodeRequired { get; set; }

        [JsonProperty("pos_tenders_credit_card_types")]
        public string PosTendersCreditCardTypes { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_allow_cacshback_on_credit_cards")]
        public string PosTendersCreditDebitCardAllowCacshbackOnCreditCards { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_allow_cacshback_on_debit_cards")]
        public string PosTendersCreditDebitCardAllowCacshbackOnDebitCards { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_allow_card_information_to_be_keyed_in")]
        public string PosTendersCreditDebitCardAllowCardInformationToBeKeyedIn { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_allow_eft_receipts_to_print_when_not_in_use")]
        public string PosTendersCreditDebitCardAllowEftReceiptsToPrintWhenNotInUse { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_max_cacshback_amt_on_credit_cards")]
        public string PosTendersCreditDebitCardMaxCacshbackAmtOnCreditCards { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_max_cacshback_amt_on_debit_cards")]
        public string PosTendersCreditDebitCardMaxCacshbackAmtOnDebitCards { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_require_card_verification")]
        public string PosTendersCreditDebitCardRequireCardVerification { get; set; }

        [JsonProperty("pos_tenders_credit_debit_card_verification_method")]
        public string PosTendersCreditDebitCardVerificationMethod { get; set; }

        [JsonProperty("pos_tenders_customtender_labels")]
        public string PosTendersCustomtenderLabels { get; set; }

        [JsonProperty("pos_tenders_default_give_tender")]
        public string PosTendersDefaultGiveTender { get; set; }

        [JsonProperty("pos_tenders_default_take_tender")]
        public string PosTendersDefaultTakeTender { get; set; }

        [JsonProperty("pos_tenders_eft_check_default_entry_method")]
        public string PosTendersEftCheckDefaultEntryMethod { get; set; }

        [JsonProperty("pos_tenders_eft_credit_default_entry_method")]
        public string PosTendersEftCreditDefaultEntryMethod { get; set; }

        [JsonProperty("pos_tenders_eft_debit_default_entry_method")]
        public string PosTendersEftDebitDefaultEntryMethod { get; set; }

        [JsonProperty("pos_tenders_eft_gift_default_entry_method")]
        public string PosTendersEftGiftDefaultEntryMethod { get; set; }

        [JsonProperty("pos_tenders_eft_use_for_check")]
        public string PosTendersEftUseForCheck { get; set; }

        [JsonProperty("pos_tenders_eft_use_for_credit")]
        public string PosTendersEftUseForCredit { get; set; }

        [JsonProperty("pos_tenders_eft_use_for_debit")]
        public string PosTendersEftUseForDebit { get; set; }

        [JsonProperty("pos_tenders_eft_use_for_gift_card")]
        public string PosTendersEftUseForGiftCard { get; set; }

        [JsonProperty("pos_tenders_gift_cards_gift_certs_auto_generate_gc_id_numbers")]
        public string PosTendersGiftCardsGiftCertsAutoGenerateGcIdNumbers { get; set; }

        [JsonProperty("pos_tenders_gift_cards_gift_certs_auto_generate_gcert_id_number")]
        public string PosTendersGiftCardsGiftCertsAutoGenerateGcertIdNumber { get; set; }

        [JsonProperty("pos_tenders_gift_cards_number_max_length")]
        public string PosTendersGiftCardsNumberMaxLength { get; set; }

        [JsonProperty("pos_tenders_given_fields_cash")]
        public object PosTendersGivenFieldsCash { get; set; }

        [JsonProperty("pos_tenders_given_fields_central_gift_card")]
        public object PosTendersGivenFieldsCentralGiftCard { get; set; }

        [JsonProperty("pos_tenders_given_fields_central_gift_certificate")]
        public object PosTendersGivenFieldsCentralGiftCertificate { get; set; }

        [JsonProperty("pos_tenders_given_fields_central_gift_credit")]
        public object PosTendersGivenFieldsCentralGiftCredit { get; set; }

        [JsonProperty("pos_tenders_given_fields_charge")]
        public object PosTendersGivenFieldsCharge { get; set; }

        [JsonProperty("pos_tenders_given_fields_check")]
        public object PosTendersGivenFieldsCheck { get; set; }

        [JsonProperty("pos_tenders_given_fields_cod")]
        public object PosTendersGivenFieldsCod { get; set; }

        [JsonProperty("pos_tenders_given_fields_credit_card")]
        public object PosTendersGivenFieldsCreditCard { get; set; }

        [JsonProperty("pos_tenders_given_fields_customer_loyalty")]
        public object PosTendersGivenFieldsCustomerLoyalty { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender1")]
        public object PosTendersGivenFieldsCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender10")]
        public object PosTendersGivenFieldsCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender2")]
        public object PosTendersGivenFieldsCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender3")]
        public object PosTendersGivenFieldsCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender4")]
        public object PosTendersGivenFieldsCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender5")]
        public object PosTendersGivenFieldsCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender6")]
        public object PosTendersGivenFieldsCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender7")]
        public object PosTendersGivenFieldsCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender8")]
        public object PosTendersGivenFieldsCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_given_fields_customtender9")]
        public object PosTendersGivenFieldsCustomtender9 { get; set; }

        [JsonProperty("pos_tenders_given_fields_debit_card")]
        public object PosTendersGivenFieldsDebitCard { get; set; }

        [JsonProperty("pos_tenders_given_fields_deposit")]
        public object PosTendersGivenFieldsDeposit { get; set; }

        [JsonProperty("pos_tenders_given_fields_foreign_check")]
        public object PosTendersGivenFieldsForeignCheck { get; set; }

        [JsonProperty("pos_tenders_given_fields_foreign_currency")]
        public object PosTendersGivenFieldsForeignCurrency { get; set; }

        [JsonProperty("pos_tenders_given_fields_gift_card")]
        public object PosTendersGivenFieldsGiftCard { get; set; }

        [JsonProperty("pos_tenders_given_fields_gift_certificicate")]
        public object PosTendersGivenFieldsGiftCertificicate { get; set; }

        [JsonProperty("pos_tenders_given_fields_payments")]
        public object PosTendersGivenFieldsPayments { get; set; }

        [JsonProperty("pos_tenders_given_fields_store_credit")]
        public object PosTendersGivenFieldsStoreCredit { get; set; }

        [JsonProperty("pos_tenders_given_fields_travelers_check")]
        public object PosTendersGivenFieldsTravelersCheck { get; set; }

        [JsonProperty("pos_tenders_open_as_modal")]
        public string PosTendersOpenAsModal { get; set; }

        [JsonProperty("pos_tenders_return_fields_cash")]
        public object PosTendersReturnFieldsCash { get; set; }

        [JsonProperty("pos_tenders_return_fields_central_gift_card")]
        public object PosTendersReturnFieldsCentralGiftCard { get; set; }

        [JsonProperty("pos_tenders_return_fields_central_gift_certificate")]
        public object PosTendersReturnFieldsCentralGiftCertificate { get; set; }

        [JsonProperty("pos_tenders_return_fields_central_gift_credit")]
        public object PosTendersReturnFieldsCentralGiftCredit { get; set; }

        [JsonProperty("pos_tenders_return_fields_charge")]
        public object PosTendersReturnFieldsCharge { get; set; }

        [JsonProperty("pos_tenders_return_fields_check")]
        public string PosTendersReturnFieldsCheck { get; set; }

        [JsonProperty("pos_tenders_return_fields_cod")]
        public object PosTendersReturnFieldsCod { get; set; }

        [JsonProperty("pos_tenders_return_fields_credit_card")]
        public string PosTendersReturnFieldsCreditCard { get; set; }

        [JsonProperty("pos_tenders_return_fields_customer_loyalty")]
        public string PosTendersReturnFieldsCustomerLoyalty { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender1")]
        public object PosTendersReturnFieldsCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender10")]
        public object PosTendersReturnFieldsCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender2")]
        public object PosTendersReturnFieldsCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender3")]
        public object PosTendersReturnFieldsCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender4")]
        public object PosTendersReturnFieldsCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender5")]
        public object PosTendersReturnFieldsCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender6")]
        public object PosTendersReturnFieldsCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender7")]
        public object PosTendersReturnFieldsCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender8")]
        public object PosTendersReturnFieldsCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_return_fields_customtender9")]
        public object PosTendersReturnFieldsCustomtender9 { get; set; }

        [JsonProperty("pos_tenders_return_fields_debit_card")]
        public string PosTendersReturnFieldsDebitCard { get; set; }

        [JsonProperty("pos_tenders_return_fields_deposit")]
        public string PosTendersReturnFieldsDeposit { get; set; }

        [JsonProperty("pos_tenders_return_fields_foreign_check")]
        public string PosTendersReturnFieldsForeignCheck { get; set; }

        [JsonProperty("pos_tenders_return_fields_foreign_currency")]
        public string PosTendersReturnFieldsForeignCurrency { get; set; }

        [JsonProperty("pos_tenders_return_fields_gift_card")]
        public object PosTendersReturnFieldsGiftCard { get; set; }

        [JsonProperty("pos_tenders_return_fields_gift_certificicate")]
        public object PosTendersReturnFieldsGiftCertificicate { get; set; }

        [JsonProperty("pos_tenders_return_fields_payments")]
        public string PosTendersReturnFieldsPayments { get; set; }

        [JsonProperty("pos_tenders_return_fields_store_credit")]
        public string PosTendersReturnFieldsStoreCredit { get; set; }

        [JsonProperty("pos_tenders_return_fields_travelers_check")]
        public object PosTendersReturnFieldsTravelersCheck { get; set; }

        [JsonProperty("pos_tenders_rules_cash")]
        public string PosTendersRulesCash { get; set; }

        [JsonProperty("pos_tenders_rules_central_gift_card")]
        public string PosTendersRulesCentralGiftCard { get; set; }

        [JsonProperty("pos_tenders_rules_central_gift_certificate")]
        public string PosTendersRulesCentralGiftCertificate { get; set; }

        [JsonProperty("pos_tenders_rules_central_gift_credit")]
        public string PosTendersRulesCentralGiftCredit { get; set; }

        [JsonProperty("pos_tenders_rules_charge")]
        public string PosTendersRulesCharge { get; set; }

        [JsonProperty("pos_tenders_rules_check")]
        public string PosTendersRulesCheck { get; set; }

        [JsonProperty("pos_tenders_rules_cod")]
        public string PosTendersRulesCod { get; set; }

        [JsonProperty("pos_tenders_rules_credit_card")]
        public string PosTendersRulesCreditCard { get; set; }

        [JsonProperty("pos_tenders_rules_customer_loyalty")]
        public string PosTendersRulesCustomerLoyalty { get; set; }

        [JsonProperty("pos_tenders_rules_customtender1")]
        public string PosTendersRulesCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender10")]
        public string PosTendersRulesCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender2")]
        public string PosTendersRulesCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender3")]
        public string PosTendersRulesCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender4")]
        public string PosTendersRulesCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender5")]
        public string PosTendersRulesCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender6")]
        public string PosTendersRulesCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender7")]
        public string PosTendersRulesCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender8")]
        public string PosTendersRulesCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_rules_customtender9")]
        public string PosTendersRulesCustomtender9 { get; set; }

        [JsonProperty("pos_tenders_rules_debit_card")]
        public string PosTendersRulesDebitCard { get; set; }

        [JsonProperty("pos_tenders_rules_deposit")]
        public string PosTendersRulesDeposit { get; set; }

        [JsonProperty("pos_tenders_rules_foreign_check")]
        public string PosTendersRulesForeignCheck { get; set; }

        [JsonProperty("pos_tenders_rules_foreign_currency")]
        public string PosTendersRulesForeignCurrency { get; set; }

        [JsonProperty("pos_tenders_rules_gift_card")]
        public string PosTendersRulesGiftCard { get; set; }

        [JsonProperty("pos_tenders_rules_gift_certificicate")]
        public string PosTendersRulesGiftCertificicate { get; set; }

        [JsonProperty("pos_tenders_rules_payments")]
        public string PosTendersRulesPayments { get; set; }

        [JsonProperty("pos_tenders_rules_store_credit")]
        public string PosTendersRulesStoreCredit { get; set; }

        [JsonProperty("pos_tenders_rules_travelers_check")]
        public string PosTendersRulesTravelersCheck { get; set; }

        [JsonProperty("pos_tenders_set_give_to_last_take")]
        public string PosTendersSetGiveToLastTake { get; set; }

        [JsonProperty("pos_tenders_store_credit_allow_cust_resue_sc_if_partial_balance_available")]
        public string PosTendersStoreCreditAllowCustResueScIfPartialBalanceAvailable { get; set; }

        [JsonProperty("pos_tenders_store_credit_auto_generate_sc_id_numbers")]
        public string PosTendersStoreCreditAutoGenerateScIdNumbers { get; set; }

        [JsonProperty("pos_tenders_store_credit_max_sc_amt_store_tab")]
        public string PosTendersStoreCreditMaxScAmtStoreTab { get; set; }

        [JsonProperty("pos_tenders_store_credit_suggest_sc_tender")]
        public string PosTendersStoreCreditSuggestScTender { get; set; }

        [JsonProperty("pos_tenders_taken_fields_cash")]
        public object PosTendersTakenFieldsCash { get; set; }

        [JsonProperty("pos_tenders_taken_fields_central_gift_card")]
        public object PosTendersTakenFieldsCentralGiftCard { get; set; }

        [JsonProperty("pos_tenders_taken_fields_central_gift_certificate")]
        public object PosTendersTakenFieldsCentralGiftCertificate { get; set; }

        [JsonProperty("pos_tenders_taken_fields_central_gift_credit")]
        public object PosTendersTakenFieldsCentralGiftCredit { get; set; }

        [JsonProperty("pos_tenders_taken_fields_charge")]
        public object PosTendersTakenFieldsCharge { get; set; }

        [JsonProperty("pos_tenders_taken_fields_check")]
        public object PosTendersTakenFieldsCheck { get; set; }

        [JsonProperty("pos_tenders_taken_fields_cod")]
        public object PosTendersTakenFieldsCod { get; set; }

        [JsonProperty("pos_tenders_taken_fields_credit_card")]
        public object PosTendersTakenFieldsCreditCard { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customer_loyalty")]
        public object PosTendersTakenFieldsCustomerLoyalty { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender1")]
        public object PosTendersTakenFieldsCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender10")]
        public object PosTendersTakenFieldsCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender2")]
        public object PosTendersTakenFieldsCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender3")]
        public object PosTendersTakenFieldsCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender4")]
        public object PosTendersTakenFieldsCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender5")]
        public object PosTendersTakenFieldsCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender6")]
        public object PosTendersTakenFieldsCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender7")]
        public object PosTendersTakenFieldsCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender8")]
        public object PosTendersTakenFieldsCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_customtender9")]
        public object PosTendersTakenFieldsCustomtender9 { get; set; }

        [JsonProperty("pos_tenders_taken_fields_debit_card")]
        public object PosTendersTakenFieldsDebitCard { get; set; }

        [JsonProperty("pos_tenders_taken_fields_foreign_check")]
        public object PosTendersTakenFieldsForeignCheck { get; set; }

        [JsonProperty("pos_tenders_taken_fields_foreign_currency")]
        public object PosTendersTakenFieldsForeignCurrency { get; set; }

        [JsonProperty("pos_tenders_taken_fields_gift_card")]
        public object PosTendersTakenFieldsGiftCard { get; set; }

        [JsonProperty("pos_tenders_taken_fields_gift_certificicate")]
        public object PosTendersTakenFieldsGiftCertificicate { get; set; }

        [JsonProperty("pos_tenders_taken_fields_payments")]
        public object PosTendersTakenFieldsPayments { get; set; }

        [JsonProperty("pos_tenders_taken_fields_store_credit")]
        public object PosTendersTakenFieldsStoreCredit { get; set; }

        [JsonProperty("pos_tenders_taken_fields_travelers_check")]
        public object PosTendersTakenFieldsTravelersCheck { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender1")]
        public string PosTendersUdfGivenCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender10")]
        public string PosTendersUdfGivenCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender2")]
        public string PosTendersUdfGivenCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender3")]
        public string PosTendersUdfGivenCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender4")]
        public string PosTendersUdfGivenCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender5")]
        public string PosTendersUdfGivenCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender6")]
        public string PosTendersUdfGivenCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender7")]
        public string PosTendersUdfGivenCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender8")]
        public string PosTendersUdfGivenCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_udf_given_customtender9")]
        public string PosTendersUdfGivenCustomtender9 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender1")]
        public object PosTendersUdfLabelsCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender10")]
        public object PosTendersUdfLabelsCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender2")]
        public object PosTendersUdfLabelsCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender3")]
        public object PosTendersUdfLabelsCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender4")]
        public object PosTendersUdfLabelsCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender5")]
        public object PosTendersUdfLabelsCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender6")]
        public object PosTendersUdfLabelsCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender7")]
        public object PosTendersUdfLabelsCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender8")]
        public object PosTendersUdfLabelsCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_udf_labels_customtender9")]
        public object PosTendersUdfLabelsCustomtender9 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender1")]
        public string PosTendersUdfTakenCustomtender1 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender10")]
        public string PosTendersUdfTakenCustomtender10 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender2")]
        public string PosTendersUdfTakenCustomtender2 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender3")]
        public string PosTendersUdfTakenCustomtender3 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender4")]
        public string PosTendersUdfTakenCustomtender4 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender5")]
        public string PosTendersUdfTakenCustomtender5 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender6")]
        public string PosTendersUdfTakenCustomtender6 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender7")]
        public string PosTendersUdfTakenCustomtender7 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender8")]
        public string PosTendersUdfTakenCustomtender8 { get; set; }

        [JsonProperty("pos_tenders_udf_taken_customtender9")]
        public string PosTendersUdfTakenCustomtender9 { get; set; }

        [JsonProperty("postal_code_city")]
        public string PostalCodeCity { get; set; }

        [JsonProperty("postal_code_enable")]
        public string PostalCodeEnable { get; set; }

        [JsonProperty("postal_code_state")]
        public string PostalCodeState { get; set; }

        [JsonProperty("postal_code_state_format")]
        public string PostalCodeStateFormat { get; set; }

        [JsonProperty("price_plan_authority")]
        public object PricePlanAuthority { get; set; }

        [JsonProperty("promotions_activate_gift_cards_for_original_amount")]
        public string PromotionsActivateGiftCardsForOriginalAmount { get; set; }

        [JsonProperty("prompt_before_expiration")]
        public string PromptBeforeExpiration { get; set; }

        [JsonProperty("prompt_for_customer_share_type")]
        public string PromptForCustomerShareType { get; set; }

        [JsonProperty("purchase_order_instruction_five")]
        public object PurchaseOrderInstructionFive { get; set; }

        [JsonProperty("purchase_order_instruction_four")]
        public object PurchaseOrderInstructionFour { get; set; }

        [JsonProperty("purchase_order_instruction_one")]
        public object PurchaseOrderInstructionOne { get; set; }

        [JsonProperty("purchase_order_instruction_three")]
        public object PurchaseOrderInstructionThree { get; set; }

        [JsonProperty("purchase_order_instruction_two")]
        public object PurchaseOrderInstructionTwo { get; set; }

        [JsonProperty("purchase_order_voucher_fee_types")]
        public object PurchaseOrderVoucherFeeTypes { get; set; }

        [JsonProperty("purchase_order_voucher_instructions")]
        public object PurchaseOrderVoucherInstructions { get; set; }

        [JsonProperty("purchasing_after_voucher_updated_go_to")]
        public string PurchasingAfterVoucherUpdatedGoTo { get; set; }

        [JsonProperty("purchasing_allow_negative_quantities")]
        public string PurchasingAllowNegativeQuantities { get; set; }

        [JsonProperty("purchasing_allow_negative_quantities_on_vouchers")]
        public string PurchasingAllowNegativeQuantitiesOnVouchers { get; set; }

        [JsonProperty("purchasing_allow_receiving_after_cancel_date")]
        public string PurchasingAllowReceivingAfterCancelDate { get; set; }

        [JsonProperty("purchasing_allow_voucher_update_inventory_prices")]
        public string PurchasingAllowVoucherUpdateInventoryPrices { get; set; }

        [JsonProperty("purchasing_case_rounding_method")]
        public string PurchasingCaseRoundingMethod { get; set; }

        [JsonProperty("purchasing_cost_to_use")]
        public string PurchasingCostToUse { get; set; }

        [JsonProperty("purchasing_inventory_costing_method")]
        public string PurchasingInventoryCostingMethod { get; set; }

        [JsonProperty("purchasing_limit_voucher_against_po_only_items_on_po")]
        public string PurchasingLimitVoucherAgainstPoOnlyItemsOnPo { get; set; }

        [JsonProperty("purchasing_order_by_case_only")]
        public string PurchasingOrderByCaseOnly { get; set; }

        [JsonProperty("purchasing_require_all_specified_packages_to_be_received")]
        public string PurchasingRequireAllSpecifiedPackagesToBeReceived { get; set; }

        [JsonProperty("purchasing_require_number_of_packages_to_be_received")]
        public string PurchasingRequireNumberOfPackagesToBeReceived { get; set; }

        [JsonProperty("purchasing_require_receive_voucher_reference_po")]
        public string PurchasingRequireReceiveVoucherReferencePo { get; set; }

        [JsonProperty("purchasing_require_return_voucher_reference_po")]
        public string PurchasingRequireReturnVoucherReferencePo { get; set; }

        [JsonProperty("purchasing_restrict_one_vendor_per_po_voucher")]
        public string PurchasingRestrictOneVendorPerPoVoucher { get; set; }

        [JsonProperty("purchasing_use_vendor_invoices")]
        public string PurchasingUseVendorInvoices { get; set; }

        [JsonProperty("purchasing_vouchers_consolidate_like_items")]
        public string PurchasingVouchersConsolidateLikeItems { get; set; }

        [JsonProperty("regional_inventory_create_departments_as_regional")]
        public string RegionalInventoryCreateDepartmentsAsRegional { get; set; }

        [JsonProperty("regional_inventory_create_items_as_regional")]
        public string RegionalInventoryCreateItemsAsRegional { get; set; }

        [JsonProperty("regional_inventory_create_vendors_as_regional")]
        public string RegionalInventoryCreateVendorsAsRegional { get; set; }

        [JsonProperty("regional_inventory_propagate_item_alu")]
        public string RegionalInventoryPropagateItemAlu { get; set; }

        [JsonProperty("regional_inventory_propagate_item_costs")]
        public string RegionalInventoryPropagateItemCosts { get; set; }

        [JsonProperty("regional_inventory_propagate_item_desc3")]
        public string RegionalInventoryPropagateItemDesc3 { get; set; }

        [JsonProperty("regional_inventory_propagate_item_desc4")]
        public string RegionalInventoryPropagateItemDesc4 { get; set; }

        [JsonProperty("regional_inventory_propagate_item_prices")]
        public string RegionalInventoryPropagateItemPrices { get; set; }

        [JsonProperty("regional_inventory_propagate_item_scale")]
        public string RegionalInventoryPropagateItemScale { get; set; }

        [JsonProperty("regional_inventory_propagate_item_taxes")]
        public string RegionalInventoryPropagateItemTaxes { get; set; }

        [JsonProperty("regional_inventory_propagate_item_udfs")]
        public string RegionalInventoryPropagateItemUdfs { get; set; }

        [JsonProperty("regional_inventory_propagate_item_unorderable")]
        public string RegionalInventoryPropagateItemUnorderable { get; set; }

        [JsonProperty("regional_inventory_propagate_item_upc")]
        public string RegionalInventoryPropagateItemUpc { get; set; }

        [JsonProperty("regional_inventory_propagate_item_use_exchangerate_for_cost")]
        public string RegionalInventoryPropagateItemUseExchangerateForCost { get; set; }

        [JsonProperty("regional_inventory_propagate_item_use_exchangerate_for_cost_rounddecimal")]
        public string RegionalInventoryPropagateItemUseExchangerateForCostRounddecimal { get; set; }

        [JsonProperty("regional_inventory_propagate_item_use_exchangerate_for_price")]
        public string RegionalInventoryPropagateItemUseExchangerateForPrice { get; set; }

        [JsonProperty("regional_inventory_propagate_item_use_exchangerate_for_price_rounddecimal")]
        public string RegionalInventoryPropagateItemUseExchangerateForPriceRounddecimal { get; set; }

        [JsonProperty("regional_inventory_propagate_vendor_udfs")]
        public string RegionalInventoryPropagateVendorUdfs { get; set; }

        [JsonProperty("regional_settings_am")]
        public string RegionalSettingsAm { get; set; }

        [JsonProperty("regional_settings_currency_gsize")]
        public string RegionalSettingsCurrencyGsize { get; set; }

        [JsonProperty("regional_settings_currency_lgsize")]
        public string RegionalSettingsCurrencyLgsize { get; set; }

        [JsonProperty("regional_settings_currency_symbol")]
        public string RegionalSettingsCurrencySymbol { get; set; }

        [JsonProperty("regional_settings_currency_symbol_position")]
        public string RegionalSettingsCurrencySymbolPosition { get; set; }

        [JsonProperty("regional_settings_custom_settings_flag")]
        public string RegionalSettingsCustomSettingsFlag { get; set; }

        [JsonProperty("regional_settings_date_format")]
        public string RegionalSettingsDateFormat { get; set; }

        [JsonProperty("regional_settings_date_time_format")]
        public string RegionalSettingsDateTimeFormat { get; set; }

        [JsonProperty("regional_settings_decimal_character")]
        public string RegionalSettingsDecimalCharacter { get; set; }

        [JsonProperty("regional_settings_decimal_spaces_cost")]
        public string RegionalSettingsDecimalSpacesCost { get; set; }

        [JsonProperty("regional_settings_decimal_spaces_currency")]
        public string RegionalSettingsDecimalSpacesCurrency { get; set; }

        [JsonProperty("regional_settings_decimal_spaces_number")]
        public string RegionalSettingsDecimalSpacesNumber { get; set; }

        [JsonProperty("regional_settings_decimal_spaces_price")]
        public string RegionalSettingsDecimalSpacesPrice { get; set; }

        [JsonProperty("regional_settings_decimal_spaces_tax")]
        public string RegionalSettingsDecimalSpacesTax { get; set; }

        [JsonProperty("regional_settings_group_separator")]
        public string RegionalSettingsGroupSeparator { get; set; }

        [JsonProperty("regional_settings_i18n")]
        public object RegionalSettingsI18N { get; set; }

        [JsonProperty("regional_settings_number_gsize")]
        public string RegionalSettingsNumberGsize { get; set; }

        [JsonProperty("regional_settings_number_lgsize")]
        public string RegionalSettingsNumberLgsize { get; set; }

        [JsonProperty("regional_settings_pm")]
        public string RegionalSettingsPm { get; set; }

        [JsonProperty("regional_settings_region")]
        public string RegionalSettingsRegion { get; set; }

        [JsonProperty("regional_settings_show_currency_symbol")]
        public string RegionalSettingsShowCurrencySymbol { get; set; }

        [JsonProperty("regional_settings_show_offset")]
        public string RegionalSettingsShowOffset { get; set; }

        [JsonProperty("regional_settings_time_format")]
        public string RegionalSettingsTimeFormat { get; set; }

        [JsonProperty("regional_settings_time_zone")]
        public string RegionalSettingsTimeZone { get; set; }

        [JsonProperty("require_check_in_for_document_creation")]
        public string RequireCheckInForDocumentCreation { get; set; }

        [JsonProperty("run_prism_standalone")]
        public string RunPrismStandalone { get; set; }

        [JsonProperty("sequencing_sequence_rules_auto_generate_po_number")]
        public string SequencingSequenceRulesAutoGeneratePoNumber { get; set; }

        [JsonProperty("sequencing_sequence_rules_use_doc_seq_on_high_security_receipts")]
        public string SequencingSequenceRulesUseDocSeqOnHighSecurityReceipts { get; set; }

        [JsonProperty("sequencing_sequence_rules_use_separate_seq_for_all_order_types")]
        public string SequencingSequenceRulesUseSeparateSeqForAllOrderTypes { get; set; }

        [JsonProperty("sequencing_sequence_rules_use_separate_seq_for_all_receipt_types")]
        public string SequencingSequenceRulesUseSeparateSeqForAllReceiptTypes { get; set; }

        [JsonProperty("sequencing_sequence_rules_use_separate_seq_for_multi_single_sbs_po")]
        public string SequencingSequenceRulesUseSeparateSeqForMultiSingleSbsPo { get; set; }

        [JsonProperty("sequencing_sequence_rules_use_separate_seq_for_receiving_return_vouchers")]
        public string SequencingSequenceRulesUseSeparateSeqForReceivingReturnVouchers { get; set; }

        [JsonProperty("set_expiration_date_upon_activation")]
        public string SetExpirationDateUponActivation { get; set; }

        [JsonProperty("set_maximum_value_upon_activation")]
        public string SetMaximumValueUponActivation { get; set; }

        [JsonProperty("simple_timeclock")]
        public string SimpleTimeclock { get; set; }

        [JsonProperty("snln_options_lot_no_expiration_alert")]
        public string SnlnOptionsLotNoExpirationAlert { get; set; }

        [JsonProperty("snln_options_prevent_fc_sn_item_sale_with_zero_qty")]
        public string SnlnOptionsPreventFcSnItemSaleWithZeroQty { get; set; }

        [JsonProperty("snln_options_prevent_item_sale_with_expired_lot_no")]
        public string SnlnOptionsPreventItemSaleWithExpiredLotNo { get; set; }

        [JsonProperty("so_pm_update_time")]
        public object SoPmUpdateTime { get; set; }

        [JsonProperty("special_orders_allow_record_sale")]
        public string SpecialOrdersAllowRecordSale { get; set; }

        [JsonProperty("struct_zout_auto_open")]
        public string StructZoutAutoOpen { get; set; }

        [JsonProperty("struct_zout_blind_close")]
        public string StructZoutBlindClose { get; set; }

        [JsonProperty("struct_zout_blind_close_attempts")]
        public string StructZoutBlindCloseAttempts { get; set; }

        [JsonProperty("struct_zout_blind_close_variance")]
        public string StructZoutBlindCloseVariance { get; set; }

        [JsonProperty("struct_zout_combine_sales_tax_for_vat")]
        public string StructZoutCombineSalesTaxForVat { get; set; }

        [JsonProperty("struct_zout_default_sort_by")]
        public string StructZoutDefaultSortBy { get; set; }

        [JsonProperty("struct_zout_enable_audits")]
        public string StructZoutEnableAudits { get; set; }

        [JsonProperty("struct_zout_end_of_day")]
        public DateTimeOffset StructZoutEndOfDay { get; set; }

        [JsonProperty("struct_zout_mode")]
        public bool StructZoutMode { get; set; }

        [JsonProperty("struct_zout_open_close_counts_required")]
        public string StructZoutOpenCloseCountsRequired { get; set; }

        [JsonProperty("struct_zout_open_define_default_open_amt_for_each_currency")]
        public string StructZoutOpenDefineDefaultOpenAmtForEachCurrency { get; set; }

        [JsonProperty("struct_zout_open_denomination_counts_required")]
        public string StructZoutOpenDenominationCountsRequired { get; set; }

        [JsonProperty("struct_zout_print_all_denominations")]
        public bool StructZoutPrintAllDenominations { get; set; }

        [JsonProperty("struct_zout_register_definition")]
        public string StructZoutRegisterDefinition { get; set; }

        [JsonProperty("struct_zout_require_daily_register_closure")]
        public string StructZoutRequireDailyRegisterClosure { get; set; }

        [JsonProperty("struct_zout_use_legacy_zout")]
        public string StructZoutUseLegacyZout { get; set; }

        [JsonProperty("success_toast_timeout")]
        public string SuccessToastTimeout { get; set; }

        [JsonProperty("taxes_apply_customer_tax_areas")]
        public string TaxesApplyCustomerTaxAreas { get; set; }

        [JsonProperty("taxes_apply_detax_when")]
        public string TaxesApplyDetaxWhen { get; set; }

        [JsonProperty("taxes_general_1st_tax_area_threshold")]
        public string TaxesGeneral1StTaxAreaThreshold { get; set; }

        [JsonProperty("taxes_general_2nd_tax_area_threshold")]
        public string TaxesGeneral2NdTaxAreaThreshold { get; set; }

        [JsonProperty("taxes_general_allow_tax_rebates")]
        public string TaxesGeneralAllowTaxRebates { get; set; }

        [JsonProperty("taxes_general_apply_detax_to_fee")]
        public string TaxesGeneralApplyDetaxToFee { get; set; }

        [JsonProperty("taxes_general_apply_detax_to_shipping")]
        public string TaxesGeneralApplyDetaxToShipping { get; set; }

        [JsonProperty("taxes_general_apply_detax_to_tax_perc")]
        public string TaxesGeneralApplyDetaxToTaxPerc { get; set; }

        [JsonProperty("taxes_general_calc_tax2_from_tax1")]
        public string TaxesGeneralCalcTax2FromTax1 { get; set; }

        [JsonProperty("taxes_general_multi_tax_vat")]
        public string TaxesGeneralMultiTaxVat { get; set; }

        [JsonProperty("taxes_general_round_ext_tax_amt_up")]
        public string TaxesGeneralRoundExtTaxAmtUp { get; set; }

        [JsonProperty("taxes_general_round_tax_amt")]
        public string TaxesGeneralRoundTaxAmt { get; set; }

        [JsonProperty("taxes_general_round_tax_method")]
        public string TaxesGeneralRoundTaxMethod { get; set; }

        [JsonProperty("taxes_general_source_of_package_tax_info")]
        public string TaxesGeneralSourceOfPackageTaxInfo { get; set; }

        [JsonProperty("taxes_general_tax_code_subtotal_calc")]
        public string TaxesGeneralTaxCodeSubtotalCalc { get; set; }

        [JsonProperty("taxes_general_tax_method")]
        public string TaxesGeneralTaxMethod { get; set; }

        [JsonProperty("taxes_general_tax_rebate_percent")]
        public string TaxesGeneralTaxRebatePercent { get; set; }

        [JsonProperty("taxes_general_tax_rebate_threshold")]
        public string TaxesGeneralTaxRebateThreshold { get; set; }

        [JsonProperty("taxes_general_tax_use_first_to_calc_second_tax")]
        public string TaxesGeneralTaxUseFirstToCalcSecondTax { get; set; }

        [JsonProperty("taxes_general_use_pwt_for_second_tax")]
        public string TaxesGeneralUsePwtForSecondTax { get; set; }

        [JsonProperty("themes_and_layouts_default_pos_layout")]
        public string ThemesAndLayoutsDefaultPosLayout { get; set; }

        [JsonProperty("themes_and_layouts_default_theme")]
        public object ThemesAndLayoutsDefaultTheme { get; set; }

        [JsonProperty("themes_and_layouts_rp_button_image")]
        public string ThemesAndLayoutsRpButtonImage { get; set; }

        [JsonProperty("themes_and_layouts_view_path")]
        public string ThemesAndLayoutsViewPath { get; set; }

        [JsonProperty("touch_menu_default_item_button_appearance")]
        public string TouchMenuDefaultItemButtonAppearance { get; set; }

        [JsonProperty("touch_menu_default_main_menu_button_label")]
        public string TouchMenuDefaultMainMenuButtonLabel { get; set; }

        [JsonProperty("touch_menu_default_menu_sid")]
        public string TouchMenuDefaultMenuSid { get; set; }

        [JsonProperty("touch_menu_default_navigation_button_appearance")]
        public string TouchMenuDefaultNavigationButtonAppearance { get; set; }

        [JsonProperty("touch_menu_default_next_page_button_label")]
        public string TouchMenuDefaultNextPageButtonLabel { get; set; }

        [JsonProperty("touch_menu_default_previous_menu_button_label")]
        public string TouchMenuDefaultPreviousMenuButtonLabel { get; set; }

        [JsonProperty("touch_menu_default_previous_page_button_label")]
        public string TouchMenuDefaultPreviousPageButtonLabel { get; set; }

        [JsonProperty("touch_menu_label_field")]
        public string TouchMenuLabelField { get; set; }

        [JsonProperty("touch_menu_page_size")]
        public string TouchMenuPageSize { get; set; }

        [JsonProperty("transactions_deposits_min_customer_order_perc")]
        public string TransactionsDepositsMinCustomerOrderPerc { get; set; }

        [JsonProperty("transactions_deposits_min_customer_order_required")]
        public string TransactionsDepositsMinCustomerOrderRequired { get; set; }

        [JsonProperty("transactions_deposits_min_layaway_order_perc")]
        public string TransactionsDepositsMinLayawayOrderPerc { get; set; }

        [JsonProperty("transactions_deposits_min_layaway_order_required")]
        public string TransactionsDepositsMinLayawayOrderRequired { get; set; }

        [JsonProperty("transactions_deposits_min_special_order_perc")]
        public string TransactionsDepositsMinSpecialOrderPerc { get; set; }

        [JsonProperty("transactions_deposits_min_special_order_required")]
        public string TransactionsDepositsMinSpecialOrderRequired { get; set; }

        [JsonProperty("transactions_general_advanced_item_lookup_by")]
        public string TransactionsGeneralAdvancedItemLookupBy { get; set; }

        [JsonProperty("transactions_general_after_trans_update_goto")]
        public string TransactionsGeneralAfterTransUpdateGoto { get; set; }

        [JsonProperty("transactions_general_alert_cashier_when_price_less_than_cost")]
        public string TransactionsGeneralAlertCashierWhenPriceLessThanCost { get; set; }

        [JsonProperty("transactions_general_allow_store_credit_tender")]
        public string TransactionsGeneralAllowStoreCreditTender { get; set; }

        [JsonProperty("transactions_general_change_window_cash")]
        public string TransactionsGeneralChangeWindowCash { get; set; }

        [JsonProperty("transactions_general_change_window_central_credit")]
        public string TransactionsGeneralChangeWindowCentralCredit { get; set; }

        [JsonProperty("transactions_general_change_window_central_gift_card")]
        public string TransactionsGeneralChangeWindowCentralGiftCard { get; set; }

        [JsonProperty("transactions_general_change_window_central_gift_cert")]
        public string TransactionsGeneralChangeWindowCentralGiftCert { get; set; }

        [JsonProperty("transactions_general_change_window_central_loyalty")]
        public string TransactionsGeneralChangeWindowCentralLoyalty { get; set; }

        [JsonProperty("transactions_general_change_window_charge")]
        public string TransactionsGeneralChangeWindowCharge { get; set; }

        [JsonProperty("transactions_general_change_window_check")]
        public string TransactionsGeneralChangeWindowCheck { get; set; }

        [JsonProperty("transactions_general_change_window_cod")]
        public string TransactionsGeneralChangeWindowCod { get; set; }

        [JsonProperty("transactions_general_change_window_credit")]
        public string TransactionsGeneralChangeWindowCredit { get; set; }

        [JsonProperty("transactions_general_change_window_customtender1")]
        public string TransactionsGeneralChangeWindowCustomtender1 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender10")]
        public string TransactionsGeneralChangeWindowCustomtender10 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender2")]
        public string TransactionsGeneralChangeWindowCustomtender2 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender3")]
        public string TransactionsGeneralChangeWindowCustomtender3 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender4")]
        public string TransactionsGeneralChangeWindowCustomtender4 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender5")]
        public string TransactionsGeneralChangeWindowCustomtender5 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender6")]
        public string TransactionsGeneralChangeWindowCustomtender6 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender7")]
        public string TransactionsGeneralChangeWindowCustomtender7 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender8")]
        public string TransactionsGeneralChangeWindowCustomtender8 { get; set; }

        [JsonProperty("transactions_general_change_window_customtender9")]
        public string TransactionsGeneralChangeWindowCustomtender9 { get; set; }

        [JsonProperty("transactions_general_change_window_debit")]
        public string TransactionsGeneralChangeWindowDebit { get; set; }

        [JsonProperty("transactions_general_change_window_foreign_currency")]
        public string TransactionsGeneralChangeWindowForeignCurrency { get; set; }

        [JsonProperty("transactions_general_change_window_gift")]
        public string TransactionsGeneralChangeWindowGift { get; set; }

        [JsonProperty("transactions_general_change_window_gift_cert")]
        public string TransactionsGeneralChangeWindowGiftCert { get; set; }

        [JsonProperty("transactions_general_change_window_payment")]
        public string TransactionsGeneralChangeWindowPayment { get; set; }

        [JsonProperty("transactions_general_change_window_store_credit")]
        public string TransactionsGeneralChangeWindowStoreCredit { get; set; }

        [JsonProperty("transactions_general_default_item_type")]
        public string TransactionsGeneralDefaultItemType { get; set; }

        [JsonProperty("transactions_general_default_order_type")]
        public string TransactionsGeneralDefaultOrderType { get; set; }

        [JsonProperty("transactions_general_enable_special_orders")]
        public string TransactionsGeneralEnableSpecialOrders { get; set; }

        [JsonProperty("transactions_general_force_logout_after_transaction")]
        public string TransactionsGeneralForceLogoutAfterTransaction { get; set; }

        [JsonProperty("transactions_general_name_of_cod_field")]
        public string TransactionsGeneralNameOfCodField { get; set; }

        [JsonProperty("transactions_general_require_ar_customer_for_fees")]
        public string TransactionsGeneralRequireArCustomerForFees { get; set; }

        [JsonProperty("transactions_general_require_customer_for_manual_discount")]
        public string TransactionsGeneralRequireCustomerForManualDiscount { get; set; }

        [JsonProperty("transactions_general_require_customer_returns")]
        public string TransactionsGeneralRequireCustomerReturns { get; set; }

        [JsonProperty("transactions_general_require_customer_sales")]
        public string TransactionsGeneralRequireCustomerSales { get; set; }

        [JsonProperty("transactions_general_restrict_order_item_rows_to_single_quantities")]
        public string TransactionsGeneralRestrictOrderItemRowsToSingleQuantities { get; set; }

        [JsonProperty("transactions_general_restrict_return_tenders_to_original_sale_tenders")]
        public string TransactionsGeneralRestrictReturnTendersToOriginalSaleTenders { get; set; }

        [JsonProperty("transactions_orders_customer_minimum_deposit")]
        public string TransactionsOrdersCustomerMinimumDeposit { get; set; }

        [JsonProperty("transactions_orders_customer_minimum_deposit_required")]
        public string TransactionsOrdersCustomerMinimumDepositRequired { get; set; }

        [JsonProperty("transactions_orders_enable_customer_orders")]
        public string TransactionsOrdersEnableCustomerOrders { get; set; }

        [JsonProperty("transactions_orders_enable_layaway_orders")]
        public string TransactionsOrdersEnableLayawayOrders { get; set; }

        [JsonProperty("transactions_orders_layaway_minimum_deposit")]
        public string TransactionsOrdersLayawayMinimumDeposit { get; set; }

        [JsonProperty("transactions_orders_layaway_minimum_deposit_required")]
        public string TransactionsOrdersLayawayMinimumDepositRequired { get; set; }

        [JsonProperty("transactions_pos_flags_default_value_pos_flag_menu1")]
        public string TransactionsPosFlagsDefaultValuePosFlagMenu1 { get; set; }

        [JsonProperty("transactions_pos_flags_default_value_pos_flag_menu2")]
        public string TransactionsPosFlagsDefaultValuePosFlagMenu2 { get; set; }

        [JsonProperty("transactions_pos_flags_default_value_pos_flag_menu3")]
        public string TransactionsPosFlagsDefaultValuePosFlagMenu3 { get; set; }

        [JsonProperty("transactions_pos_flags_menu_one_required")]
        public string TransactionsPosFlagsMenuOneRequired { get; set; }

        [JsonProperty("transactions_pos_flags_menu_three_required")]
        public string TransactionsPosFlagsMenuThreeRequired { get; set; }

        [JsonProperty("transactions_pos_flags_menu_two_required")]
        public string TransactionsPosFlagsMenuTwoRequired { get; set; }

        [JsonProperty("transactions_promos_apply_automatically_or_manually")]
        public string TransactionsPromosApplyAutomaticallyOrManually { get; set; }

        [JsonProperty("transactions_promos_apply_before_tendering")]
        public string TransactionsPromosApplyBeforeTendering { get; set; }

        [JsonProperty("transactions_promos_enable")]
        public bool TransactionsPromosEnable { get; set; }

        [JsonProperty("transactions_promos_field_storing_manual_disc")]
        public object TransactionsPromosFieldStoringManualDisc { get; set; }

        [JsonProperty("transactions_promos_new_or_legacy_promos")]
        public string TransactionsPromosNewOrLegacyPromos { get; set; }

        [JsonProperty("transactions_promos_unique_names")]
        public string TransactionsPromosUniqueNames { get; set; }

        [JsonProperty("transactions_promos_use_manually_discounted_items")]
        public string TransactionsPromosUseManuallyDiscountedItems { get; set; }

        [JsonProperty("transactions_promos_use_predefined_discount_reasons")]
        public string TransactionsPromosUsePredefinedDiscountReasons { get; set; }

        [JsonProperty("transactions_returns_default_item_return_reason")]
        public string TransactionsReturnsDefaultItemReturnReason { get; set; }

        [JsonProperty("transactions_returns_require_reason_on_item_returns")]
        public string TransactionsReturnsRequireReasonOnItemReturns { get; set; }

        [JsonProperty("transactions_seperate_for_sales_and_orders")]
        public string TransactionsSeperateForSalesAndOrders { get; set; }

        [JsonProperty("transactions_tenders_check_available_charge_balance")]
        public string TransactionsTendersCheckAvailableChargeBalance { get; set; }

        [JsonProperty("transfer_slip_fee_types")]
        public object TransferSlipFeeTypes { get; set; }

        [JsonProperty("transfers_after_slip_updated_go_to")]
        public string TransfersAfterSlipUpdatedGoTo { get; set; }

        [JsonProperty("transfers_availability_check")]
        public string TransfersAvailabilityCheck { get; set; }

        [JsonProperty("transfers_general_consolidate_like_items")]
        public string TransfersGeneralConsolidateLikeItems { get; set; }

        [JsonProperty("transfers_general_require_slips_reference_to")]
        public string TransfersGeneralRequireSlipsReferenceTo { get; set; }

        [JsonProperty("transfers_require_comment_on_slip")]
        public string TransfersRequireCommentOnSlip { get; set; }

        [JsonProperty("ts_default_resolution_method")]
        public string TsDefaultResolutionMethod { get; set; }

        [JsonProperty("ts_generate_doc_upon_update")]
        public string TsGenerateDocUponUpdate { get; set; }

        [JsonProperty("ts_resolution_rules")]
        public string TsResolutionRules { get; set; }

        [JsonProperty("ts_verify_transfers_upon_voucher_update")]
        public string TsVerifyTransfersUponVoucherUpdate { get; set; }

        [JsonProperty("update_order_cost_when_making_vouchers")]
        public string UpdateOrderCostWhenMakingVouchers { get; set; }

        [JsonProperty("use_single_sequence_for_all_order_types")]
        public string UseSingleSequenceForAllOrderTypes { get; set; }

        [JsonProperty("use_single_sequence_for_return_and_sales")]
        public string UseSingleSequenceForReturnAndSales { get; set; }

        [JsonProperty("vouchers_enable_vendor_invoice")]
        public string VouchersEnableVendorInvoice { get; set; }

        [JsonProperty("warning_toast_timeout")]
        public string WarningToastTimeout { get; set; }

        [JsonProperty("workstation_types")]
        public string WorkstationTypes { get; set; }

        [JsonProperty("xzout_print_itemized_central_credit_tenders")]
        public string XzoutPrintItemizedCentralCreditTenders { get; set; }

        [JsonProperty("xzout_print_itemized_central_gift_card_tenders")]
        public string XzoutPrintItemizedCentralGiftCardTenders { get; set; }

        [JsonProperty("xzout_print_itemized_central_gift_certificate_tenders")]
        public string XzoutPrintItemizedCentralGiftCertificateTenders { get; set; }

        [JsonProperty("xzout_print_itemized_charge_tenders")]
        public string XzoutPrintItemizedChargeTenders { get; set; }

        [JsonProperty("xzout_print_itemized_check_tenders")]
        public string XzoutPrintItemizedCheckTenders { get; set; }

        [JsonProperty("xzout_print_itemized_cod_tenders")]
        public string XzoutPrintItemizedCodTenders { get; set; }

        [JsonProperty("xzout_print_itemized_credit_card_tenders")]
        public string XzoutPrintItemizedCreditCardTenders { get; set; }

        [JsonProperty("xzout_print_itemized_customer_loyalty_tenders")]
        public string XzoutPrintItemizedCustomerLoyaltyTenders { get; set; }

        [JsonProperty("xzout_print_itemized_customtender1")]
        public string XzoutPrintItemizedCustomtender1 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender10")]
        public string XzoutPrintItemizedCustomtender10 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender2")]
        public string XzoutPrintItemizedCustomtender2 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender3")]
        public string XzoutPrintItemizedCustomtender3 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender4")]
        public string XzoutPrintItemizedCustomtender4 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender5")]
        public string XzoutPrintItemizedCustomtender5 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender6")]
        public string XzoutPrintItemizedCustomtender6 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender7")]
        public string XzoutPrintItemizedCustomtender7 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender8")]
        public string XzoutPrintItemizedCustomtender8 { get; set; }

        [JsonProperty("xzout_print_itemized_customtender9")]
        public string XzoutPrintItemizedCustomtender9 { get; set; }

        [JsonProperty("xzout_print_itemized_debit_card_tenders")]
        public string XzoutPrintItemizedDebitCardTenders { get; set; }

        [JsonProperty("xzout_print_itemized_deposit_tenders")]
        public string XzoutPrintItemizedDepositTenders { get; set; }

        [JsonProperty("xzout_print_itemized_foreign_currency_check_tenders")]
        public string XzoutPrintItemizedForeignCurrencyCheckTenders { get; set; }

        [JsonProperty("xzout_print_itemized_gift_card_tenders")]
        public string XzoutPrintItemizedGiftCardTenders { get; set; }

        [JsonProperty("xzout_print_itemized_gift_certificate_tenders")]
        public string XzoutPrintItemizedGiftCertificateTenders { get; set; }

        [JsonProperty("xzout_print_itemized_payments_tenders")]
        public string XzoutPrintItemizedPaymentsTenders { get; set; }

        [JsonProperty("xzout_print_itemized_store_credit_tenders")]
        public string XzoutPrintItemizedStoreCreditTenders { get; set; }

        [JsonProperty("xzout_print_itemized_traveler_check_tenders")]
        public string XzoutPrintItemizedTravelerCheckTenders { get; set; }

        [JsonProperty("zout_leave_define_each_currency_amount")]
        public object ZoutLeaveDefineEachCurrencyAmount { get; set; }

        [JsonProperty("zout_maximum_balance")]
        public string ZoutMaximumBalance { get; set; }

        [JsonProperty("zout_maximum_balance_enabled")]
        public string ZoutMaximumBalanceEnabled { get; set; }

        [JsonProperty("zout_open_denomination_counts_required")]
        public string ZoutOpenDenominationCountsRequired { get; set; }

        [JsonProperty("zout_require_finalization_before_reopen")]
        public bool ZoutRequireFinalizationBeforeReopen { get; set; }

        [JsonProperty("zout_use_sequencing")]
        public string ZoutUseSequencing { get; set; }
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

    public class SidNo
    {
        public string SID { get; set; }
    }

}
