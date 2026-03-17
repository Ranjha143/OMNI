using Dapper;
using Microsoft.AspNetCore.SignalR.Protocol;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMNI;
using OMNI.Shopify.DataModels;
using OMNI_Dashboard.ApiControllers;
using Oracle.ManagedDataAccess.Client;
using PluginManager;
using Quartz;
using RestSharp;
using RetailPro_V22;
using RetailPro2_X.BL;
//using Shopify;

//using Shopify;
using ShopifySharp.GraphQL;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;


namespace RetailPro2_X
{
    
    [DisallowConcurrentExecution]
    internal class SaleOrder : IJob
    {
        private DateTime? lastFetchDateTime = null;
        private SubsidiaryInfo subsidiary_info = new SubsidiaryInfo();
        private BsonDocument logDocument = new BsonDocument();

        private List<PaymentGatewayInfo> PaymentGatewayInfoList = [];
        //private readonly BackgroundWorker threadWorker = new BackgroundWorker
        //{
        //    WorkerReportsProgress = true,
        //    WorkerSupportsCancellation = true
        //};
        //public SaleOrder()
        //{
        //    //threadWorker.DoWork += ThreadWorker_DoWork;
        //    //threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
        //    //threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;
        //}

        public async Task Execute(IJobExecutionContext context)
        {
            //if (!GlobalVariables.OrderServiceIsEnabled)
            //    return;

            if (GlobalVariables.RetailProOrderWorker)
                return;


            //var workerTask = Task.Factory.StartNew(() => LoadConfigurations().Wait());
            //Task.WaitAll(workerTask);
            GlobalVariables.RetailProOrderWorker = true;
            await LoadConfigurations();
            await ProcessOrders();
            GlobalVariables.RetailProOrderWorker = false;
            //threadWorker.RunWorkerAsync();

        }

        private async Task<bool> LoadConfigurations()
        {
            MongoClient mongoDbClient = new MongoClient(GlobalVariables.MongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);
            IMongoCollection<BsonDocument> servicesCollection = mongoDB.GetCollection<BsonDocument>("omni_services");
            var serviceFilter = $@"{{""service"":""Order""}}";
            var serviceResult = await servicesCollection.Find(serviceFilter).ToListAsync();
            if (serviceResult.Any())
            {
                var serviceObject = serviceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var ServiceInfo = JsonConvert.DeserializeObject<List<ServiceConfigurationInfo>>(JsonConvert.SerializeObject(serviceObject))?.FirstOrDefault();
                GlobalVariables.OrderServiceIsEnabled = ServiceInfo?.Enabled??false;
            }

            IMongoCollection<PaymentGatewayInfo> PaymentGatewayCollection = mongoDB.GetCollection<PaymentGatewayInfo>("payment_gateway_account_ids");
            PaymentGatewayInfoList = await PaymentGatewayCollection.Find(_ => true).ToListAsync();

            return true;
        }

        //private void ThreadWorker_DoWork(object? sender, DoWorkEventArgs e)
        //{
        //    var workerTask = Task.Factory.StartNew(() => ProcessOrders().Wait());
        //    Task.WaitAll(workerTask);
        //    //var PosSaleInvoiceWorkerTask = Task.Factory.StartNew(() => PosSaleInvoice().Wait());
        //    //Task.WaitAll(PosSaleInvoiceWorkerTask);
        //}
        //private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        //{
        //}
        //private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        //{
        //    GlobalVariables.RetailProOrderWorker = false;
        //}

        private async Task<bool> ProcessOrders()
        {
            try
            {
                string mongoConnectionString = GlobalVariables.MongoConnectionString; ;
                MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
                var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

                var retailproConfig = GlobalVariables.RProConfig;

#if DEBUG
                //var refundWorkerTask = Task.Factory.StartNew(() => PosRefundInvoice().Wait());
                //Task.WaitAll(refundWorkerTask);
                var soWorkerTask = Task.Factory.StartNew(() => PostDirectSaleInvoice().Wait());
                Task.WaitAll(soWorkerTask);
#endif

#if !DEBUG
                var stockTransferWorkerTask = Task.Factory.StartNew(() => TransferStock().Wait());
                Task.WaitAll(stockTransferWorkerTask);

                var soWorkerTask = Task.Factory.StartNew(() => PostDirectSaleInvoice().Wait());
                Task.WaitAll(soWorkerTask);

                var refundWorkerTask = Task.Factory.StartNew(() => PosRefundInvoice().Wait());
                Task.WaitAll(refundWorkerTask);

                if (retailproConfig.SaleCancellationDirection == "pull")
                {
                    var cancellationWorkerTask = Task.Factory.StartNew(() => PosCancelSaleOrder().Wait());
                    Task.WaitAll(cancellationWorkerTask);
                }
#endif
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> TransferStock()
        {
            string mongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoClient mongoDbClient = new(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            var OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
            var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");

            var countryCollection = mongoDB.GetCollection<BsonDocument>("countries");
            var retailproInvCollection = mongoDB.GetCollection<BsonDocument>("retailpro_inventory");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {

                string retailProSid = string.Empty;

                var storeInfoQuery = @$"
                        Select to_char(sid) as SID,STORE_NAME, STORE_CODE, STORE_NO, to_char(ACTIVE_PRICE_LVL_SID) as ACTIVE_PRICE_LVL_SID, to_char(SBS_SID) as SBS_SID 
                        from Rps.Store where store_no = {GlobalVariables.RProConfig.OrderStoreNo} 
                        AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})
                    ";
                StoreInfo storeInfo = ADO.ReadAsync<StoreInfo>(storeInfoQuery)?.FirstOrDefault() ?? new();

                var filterQuery = "{posted:false, has_error:false, stock_transfered:false, accepted_by_store:'accepted', is_courier_assigned:true}";
                var orderResult = await OrderCollection.Find(filterQuery).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var orderList = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderObj))?.ToList() ?? [];

                var rpcCallUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/security/altersession?action=changevalues"; //

                if (orderList.Count > 0)
                {
                    foreach (var OrderInfo in orderList)
                    {
                    RetryAuthorization:
                        if (!String.IsNullOrEmpty(GlobalVariables.RetailProAuthSession))
                        {
                            var RetailProRPCToken = GlobalVariables.RetailProAuthSession;

                            if (OrderInfo.PaymentGatewayNames.Count > 1)
                            {
                                OrderInfo.PaymentGatewayNames = OrderInfo.Transactions.Where(t => !t.Status.ToUpper().Contains("FAIL")).Select(t => t.FormattedGateway).ToList();
                            }

                            var OrderValidaterQuery = $"Select To_Char(sid) as sid from Rps.Document where NOTES_GENERAL = '{OrderInfo.Name}' " +
                                                      $" AND subsidiary_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                            var ExistingOrderSid = ADO.ReadAsync<SidNo>(OrderValidaterQuery).FirstOrDefault();

                            if (ExistingOrderSid != null)
                            {
                                continue;
                            }

                            try
                            {
                                var Payload = $@"{{""subsidiarysid"":""{storeInfo.SBS_SID}"",""storesid"":""{storeInfo.SID}""}}";// $@"[{{""MethodName"":""ChangeSubStoreMethod"",""Params"":{{""SubsidiarySid"":""{store_info.SBS_SID}"",""StoreSid"":""{store_info.STORE_SID}""}}}}]";
                                var rpcResult = await APICall.Post(rpcCallUrl, Payload, GlobalVariables.RetailProAuthSession);

                                if (rpcResult.IsSuccessful)
                                {
                                    var rpcResultInfo = JsonConvert.DeserializeObject<List<RpcResult>>(rpcResult.Content).FirstOrDefault();
                                    RetailProRPCToken = rpcResultInfo.Token;
                                }
                                else if (rpcResult.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    await Task.Delay(3000);
                                    GlobalVariables.RetailProAuthSession = string.Empty;
                                    goto RetryAuthorization;
                                }
                                else if (rpcResult.Content.Contains("Authorization failed"))
                                {
                                    await Task.Delay(3000);
                                    GlobalVariables.RetailProAuthSession = string.Empty;
                                    goto RetryAuthorization;
                                }
                                else
                                {
                                    var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", rpcResult.Content);
                                    OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                    BsonDocument document = new BsonDocument();
                                    document["created_at"] = DateTime.Now;
                                    document["service"] = "Shopify Orders";
                                    document["exception_message"] = "Authorization Error : " + rpcResult.Content;
                                    document["exception_source"] = "Retailpro RPC Call";
                                    document["exception_stack_trace"] = "";
                                    await exceptionCollection.InsertOneAsync(document);

                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", ex.Message);
                                OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                BsonDocument document = new BsonDocument();
                                document["created_at"] = DateTime.Now;
                                document["service"] = "Shopify Orders";
                                document["exception_message"] = "Error : " + ex.Message;
                                document["exception_source"] = ex.Source;
                                document["exception_stack_trace"] = ex.StackTrace;
                                await exceptionCollection.InsertOneAsync(document);

                                break;
                            }

                            var storeSidQuery = $@"select to_char(sbs_sid) as sbs_sid, store_name from rps.store where 1=1 AND sid = {OrderInfo.assigned_store_sid}";
                            JObject ourStoreInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery)?.FirstOrDefault() ?? new();
                            var outStoreSid = ourStoreInfo["SBS_SID"]?.ToString() ?? "NA";

                            var items = OrderInfo.LineItemList.ToList();//.Fulfillments.SelectMany(f => f.FulfillmentLineItemList).ToList();

                            var slipItemsList = new List<SlipItemData>();
                            foreach (var item in items)
                            {
                                var itemSidQuery = $@"select to_char(sid) as SID from rps.invn_sbs_item where ALU = '{item.Sku}' 
                                                      AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO}) ";

                                JObject itemSidInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(itemSidQuery)?.FirstOrDefault() ?? new();
                                var itemSid = itemSidInfo["SID"].ToString();

                                slipItemsList.Add(new SlipItemData
                                {
                                    originapplication = "RProPrismWeb",
                                    itemsid = itemSid,
                                    slipsid = "NA",
                                    qty = item.Quantity
                                });
                            }

                            var transferSlipUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip";
                            var slipPayload = $@"
                                        {{
                                            ""data"": [
                                                {{
                                                    ""originapplication"": ""RProPrismWeb"",
                                                    ""status"": 3,
                                                    ""docreasonsid"": null,
                                                    ""insbssid"": ""{storeInfo.SBS_SID}"",
                                                    ""instoresid"": ""{storeInfo.SID}"",
                                                    ""outsbssid"": ""{outStoreSid}"",
                                                    ""outstoresid"":""{OrderInfo.assigned_store_sid}""
                                                }}
                                            ]
                                        }}
                                        ";
                            // Slip Posting
                            var slipResponse = await APICall.Post(transferSlipUrl, slipPayload, GlobalVariables.RetailProAuthSession);
                            var responseData = JsonConvert.DeserializeObject<JObject>(slipResponse.Content ?? "{}");
                            var slipSid = responseData?["data"]?[0]?["sid"]?.ToString() ?? "";

                            // Slip Item Posting
                            var transferSlitItemUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{slipSid}/slipitem";
                            slipItemsList.ForEach(i => i.slipsid = slipSid);

                            RootObject rootObject = new RootObject { data = slipItemsList };
                            var slipItemResponse = await APICall.Post(transferSlitItemUrl, JsonConvert.SerializeObject(rootObject), RetailProRPCToken);

                            var commentsUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/slipcomment?comments=Shopify Order No : {OrderInfo.Name}&slipsid={slipSid}";
                            var commentsPayload = $@"{{""data"":[{{""originapplication"":""RProPrismWeb"",""slipsid"":""{slipSid}"",""comments"":""Shopify Order No : {OrderInfo.Name}""}}]}}";
                            var commentsPostResponse = await APICall.Post(commentsUrl, commentsPayload, GlobalVariables.RetailProAuthSession);

                            // get RowVersion to finalize slip
                            var slipRowversionURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{slipSid}";
                            var slipRowversionResponse = await APICall.GetAsync(slipRowversionURL, GlobalVariables.RetailProAuthSession);
                            var slipRowversionData = JsonConvert.DeserializeObject<JObject>(slipRowversionResponse.Content ?? "{}");
                            int.TryParse(slipRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int rowversion);

                            // finalize slip
                            var slipFinalizeUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{slipSid}";
                            var slipFinalizePayload = $@"
                                        {{
                                            ""data"": [
                                                {{
                                                    ""originapplication"": ""RProPrismWeb"",
                                                    ""rowversion"": {rowversion},
                                                    ""status"": 4
                                                }}
                                            ]
                                        }}
                                        ";
                            var slipFinalized = await APICall.PUT(slipFinalizeUrl, slipFinalizePayload, GlobalVariables.RetailProAuthSession);

                            var slipfinalizedData = JsonConvert.DeserializeObject<JObject>(slipFinalized.Content ?? "{}");
                            var voucherSid = slipfinalizedData?["data"]?[0]?["vousid"]?.ToString();
                            // vousid
                            if (slipFinalized.IsSuccessful)
                            {
                                var convertAsnToVoucherURL = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving?action=convertasntovoucher";

                                var convertVoucherPayload = $@"
                                            {{
                                                ""data"": [
                                                    {{
                                                        ""clerksid"": ""745052955000115352"",
                                                        ""asnsidlist"": ""{voucherSid}"",
                                                        ""doupdatevoucher"": false,
                                                        ""originapplication"": ""RProPrismWeb""
                                                    }}
                                                ]
                                            }}
                                            ";

                                var convertResponse = await APICall.Post(convertAsnToVoucherURL, convertVoucherPayload, GlobalVariables.RetailProAuthSession);

                                //var convertData = JsonConvert.DeserializeObject<JObject>(convertResponse.Content ?? "{}");
                                //var voucherSid1 = convertData?["data"]?[0]?["sid"]?.ToString() ?? "";

                                var voucherURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving/{voucherSid}";
                                var voucherRowversionResponse = await APICall.GetAsync(voucherURL, GlobalVariables.RetailProAuthSession);
                                var voucherRowversionData = JsonConvert.DeserializeObject<JObject>(voucherRowversionResponse.Content ?? "{}");
                                int.TryParse(voucherRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int vRowversion);

                                var voucherFinalizePayload = $@"
                                            {{
                                                ""data"": [
                                                    {{
                                                        ""rowversion"": {vRowversion},
                                                        ""status"": 4,
                                                        ""approvstatus"": 2,
                                                        ""publishstatus"": 2
                                                    }}
                                                ]
                                            }}
                                            ";

                                var voucherFinalizeResponse = await APICall.PUT(voucherURL, voucherFinalizePayload, GlobalVariables.RetailProAuthSession);

                                if (voucherFinalizeResponse.IsSuccessful)
                                {
                                    var filter = $"{{name:'{OrderInfo.Name}' }}";

                                    var update = Builders<BsonDocument>.Update.Set("stock_transfered", true);
                                    var result = await OrderCollection.UpdateOneAsync(filter, update);
                                }
                            }

                        }
                        else
                        {

                            GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig.PrismUser, GlobalVariables.RProConfig.PrismPassword, "webclient");
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["created_at"] = DateTime.Now;
                document["service"] = "Shopify Orders";
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;

                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private async Task<bool> PostDirectSaleInvoice()
        {
            string mongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            var OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
            var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");

            var countryCollection = mongoDB.GetCollection<BsonDocument>("countries");
            var retailproInvCollection = mongoDB.GetCollection<BsonDocument>("retailpro_inventory");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {

                var storeSIdQuery = $"select STORE_NO, to_char(sid) as STORE_SID, to_char(sbs_sid) as SBS_SID from RPS.STORE " +
                $" where store_no = {GlobalVariables.RProConfig.OrderStoreNo}  and sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                store_Sbs_SID store_info = ADO.ReadAsync<store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new();

                var storeInfoQuery = $"Select to_char(sid) as SID,STORE_NAME, STORE_CODE, STORE_NO, to_char(ACTIVE_PRICE_LVL_SID) as ACTIVE_PRICE_LVL_SID, to_char(SBS_SID) as SBS_SID " +
                    $"from Rps.Store " +
                    $"where store_no = {store_info.STORE_NO} " +
                    $"AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                StoreInfo storeInfo = ADO.ReadAsync<StoreInfo>(storeInfoQuery)?.FirstOrDefault() ?? new();

                var filterQuery = "{posted:false, has_error:false, assigned_store_name:{$ne:null}, dispatched:true, stock_transfered:true }";
                
                var orderResult = await OrderCollection.Find(filterQuery).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var orderList = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderObj))?.ToList() ?? [];

                var rpcCallUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/security/altersession?action=changevalues";

                if (orderList.Count > 0)
                {
                    foreach (var OrderInfo in orderList)
                    {
                        if (OrderInfo.PaymentGatewayNames.Count > 1)
                        {
                            OrderInfo.PaymentGatewayNames = OrderInfo.Transactions.Where(t => !t.Status.ToUpper().Contains("FAIL")).Select(t => t.FormattedGateway).ToList();
                        }


                        if (OrderInfo.PaymentGatewayNames.Count == 1)
                        {
                            var PaymentGatewayAccountId = "";
                            // if order is cod then assaign couier name 
                            if (OrderInfo.PaymentGatewayNames.FirstOrDefault().ToLower().Contains("cod"))
                            {
                                var courierName = OrderInfo.Courier.CourierName;
                                PaymentGatewayAccountId = PaymentGatewayInfoList.Where(p => p.PaymentGatewayName == courierName).FirstOrDefault()?.PaymentGatewayAccountId ?? string.Empty;

                            }
                            else
                            {
                                 PaymentGatewayAccountId = PaymentGatewayInfoList.Where(p => p.PaymentGatewayName == OrderInfo.PaymentGatewayNames.FirstOrDefault()).FirstOrDefault()?.PaymentGatewayAccountId ?? string.Empty;

                            }




                            if (!string.IsNullOrEmpty(PaymentGatewayAccountId))
                            {
                                RetryAuthorization:
                                    if (!String.IsNullOrEmpty(GlobalVariables.RetailProAuthSession))
                                    {


                                        var RetailProRPCToken = GlobalVariables.RetailProAuthSession;
                                        long AddressCount = 0;
                                        string BtCountry = "unknown";
                                        string StCountry = "unknown";

                                        try
                                        {
                                            var Payload = $@"{{""subsidiarysid"":""{store_info.SBS_SID}"",""storesid"":""{store_info.STORE_SID}""}}";

                                            var rpcResult = await APICall.Post(rpcCallUrl, Payload, GlobalVariables.RetailProAuthSession);

                                            if (rpcResult.IsSuccessful)
                                            {
                                                var rpcResultInfo = JsonConvert.DeserializeObject<List<RpcResult>>(rpcResult.Content).FirstOrDefault();
                                                RetailProRPCToken = rpcResultInfo.Token;
                                            }
                                            else if (rpcResult.StatusCode == HttpStatusCode.Forbidden)
                                            {
                                                await Task.Delay(3000);
                                                GlobalVariables.RetailProAuthSession = string.Empty;
                                                goto RetryAuthorization;
                                            }
                                            else if (rpcResult.Content.Contains("Authorization failed"))
                                            {
                                                await Task.Delay(3000);
                                                GlobalVariables.RetailProAuthSession = string.Empty;
                                                goto RetryAuthorization;
                                            }
                                            else
                                            {
                                                var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", rpcResult.Content);
                                                OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                                BsonDocument document = new BsonDocument();
                                                document["created_at"] = DateTime.Now;
                                                document["service"] = "Shopify Orders";
                                                document["order_no"] = OrderInfo.Name;
                                                document["exception_message"] = "Error : " + rpcResult.Content;
                                                document["exception_source"] = "Retailpro RPC Call";
                                                document["exception_stack_trace"] = "";
                                                await exceptionCollection.InsertOneAsync(document);

                                                continue;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", ex.Message);
                                            OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                            BsonDocument document = new BsonDocument();
                                            document["created_at"] = DateTime.Now;
                                            document["service"] = "Shopify Orders";
                                            document["order_no"] = OrderInfo.Name;
                                            document["exception_message"] = "Invalid Shipping Description : " + ex.Message;
                                            document["exception_source"] = ex.Source;
                                            document["exception_stack_trace"] = ex.StackTrace;
                                            await exceptionCollection.InsertOneAsync(document);

                                            continue;
                                        }

                                        var BillingAddressInfo = OrderInfo.BillingAddress;
                                        var ShippingAddressInfo = OrderInfo.ShippingAddress;
                                        var customerInfo = OrderInfo.Customer;

                                        var OrderValidaterQuery = $"Select To_Char(sid) as sid from Rps.Document where NOTES_GENERAL = '{OrderInfo.Name}'" +
                                            $"AND subsidiary_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                                        var ExistingOrderSid = ADO.ReadAsync<SidNo>(OrderValidaterQuery).FirstOrDefault();

                                        if (ExistingOrderSid == null)
                                        {

                                            PostResponseObject customerPostResponseInfo = new();
                                            PostResponseObject documentPostResponseInfo = new();

                                            #region ===================================================================================================== Customer

                                            string? CustomerPhoneNumber = BillingAddressInfo.Phone != null ? BillingAddressInfo.Phone : ShippingAddressInfo.Phone != null ? ShippingAddressInfo.Phone : customerInfo.Phone;

                                            if (!string.IsNullOrEmpty(CustomerPhoneNumber))
                                            {
                                                var CustomerQuery = " select to_char(C.SID) as SID, " +
                                                    " to_char(C.CUST_ID) as CUST_ID, " +
                                                    " C.FIRST_NAME, " +
                                                    " C.LAST_NAME, " +
                                                    " P.PHONE_NO " +
                                                    " FROM Rps.customer C, " +
                                                    " RPS.Customer_Phone P " +
                                                    " WHERE C.SID = P.CUST_SID " +
                                                    " AND substr(P.PHONE_NO,-9) like substr('%" + CustomerPhoneNumber + "',-9)";

                                                var ExistingCustomer = ADO.ReadAsync<Customer_Address_info>(CustomerQuery).FirstOrDefault();


                                                if (ExistingCustomer != null)
                                                {
                                                    customerPostResponseInfo.Sid = ExistingCustomer.SID.ToString();

                                                    var completeAddress = (BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2).Trim();
                                                    completeAddress = string.IsNullOrEmpty(completeAddress) ? (ShippingAddressInfo.Address1 + " " + ShippingAddressInfo.Address2).Trim() : completeAddress;

                                                    var CustSid = ExistingCustomer.SID.ToString();
                                                    var addressCountQuery = " select Count(*) as COUNT from RPS.Customer_Address A where A.CUST_SID = " + CustSid + "";
                                                    AddressCount = ADO.ReadAsync<RecordCount>(addressCountQuery).FirstOrDefault()?.COUNT ?? 0;

                                                    var existingAddressQuery = $" select PRIMARY_FLAG, ACTIVE, ADDRESS_1, ADDRESS_2, ADDRESS_3, CITY  from RPS.Customer_Address A where CUST_SID = {ExistingCustomer.SID} AND CITY like '{BillingAddressInfo.City.ToUpper()}'";
                                                    var existingAddress = ADO.ReadAsync<Customer_Address_info>(existingAddressQuery).FirstOrDefault();

                                                    if (existingAddress == null)
                                                    {
                                                        var address_line_1 = completeAddress.Length > 40 ? completeAddress.Substring(0, 40) : completeAddress;
                                                        var address_line_2 = completeAddress.Length > 80 ? completeAddress.Substring(40, 40) : completeAddress.Length > 40 ? completeAddress.Substring(40, completeAddress.Length - 40) : "";
                                                        var address_line_3 = (completeAddress.Length > 120) ? completeAddress.Substring(80, (completeAddress.Length - 80)) : "";

                                                        var PostAddress = new List<Address>
                                                    {
                                                        new Address
                                                        {
                                                            origin_application = "OMNI",
                                                            customer_sid = CustSid,
                                                            active = true,
                                                            address_allow_contact = false,
                                                            primary_flag = true,
                                                            city = (string.IsNullOrEmpty(BillingAddressInfo.City)? ShippingAddressInfo.City : BillingAddressInfo.City)?.ToUpper()??"",
                                                            address_line_1 = address_line_1,
                                                            address_line_2 = address_line_2,
                                                            address_line_3 = address_line_3,
                                                            seq_no = ++AddressCount,
                                                            Email = customerInfo.Email??OrderInfo.Email
                                                        }
                                                    };

                                                        string customerAddressPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/customer/" + CustSid + "/address";

                                                        var customerPostResponse = await APICall.Post(customerAddressPostLink, JsonConvert.SerializeObject(PostAddress, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                                    }
                                                    else
                                                    {
                                                        var existingAddressLine = existingAddress.ADDRESS_1 + existingAddress.ADDRESS_2 + existingAddress.ADDRESS_3;

                                                        var aString1 = completeAddress.Replace(",", " ").Split(' ').ToList().OrderBy(s => s);
                                                        var aString2 = existingAddressLine.Replace(",", " ").Split(' ').ToList().OrderBy(s => s);

                                                        var sameAddress = Enumerable.SequenceEqual(aString1, aString2);
                                                        if (!sameAddress)
                                                        {
                                                            var address_line_1 = completeAddress.Length > 40 ? completeAddress.Substring(0, 40) : completeAddress;
                                                            var address_line_2 = completeAddress.Length > 80 ? completeAddress.Substring(40, 40) : completeAddress.Length > 40 ? completeAddress.Substring(40, completeAddress.Length - 40) : "";
                                                            var address_line_3 = (completeAddress.Length > 120) ? completeAddress.Substring(80, (completeAddress.Length - 80)) : "";

                                                            var PostAddress = new List<Address>
                                                    {
                                                        new Address
                                                        {
                                                            origin_application = "OMNI",
                                                            customer_sid = CustSid,
                                                            active = true,
                                                            address_allow_contact = false,
                                                            primary_flag = true,
                                                            city = (string.IsNullOrEmpty(BillingAddressInfo.City)? ShippingAddressInfo.City : BillingAddressInfo.City)?.ToUpper()??"",
                                                            address_line_1 = address_line_1,
                                                            address_line_2 = address_line_2,
                                                            address_line_3 = address_line_3,
                                                            address_line_5 = (string.IsNullOrEmpty(BillingAddressInfo.City) ? ShippingAddressInfo.City : BillingAddressInfo.City)?.ToUpper() ?? "",
                                                            seq_no = ++AddressCount,
                                                            Email = customerInfo.Email??OrderInfo.Email
                                                        }
                                                    };

                                                            string customerAddressPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/customer/" + CustSid + "/address";
                                                            var customerPostResponse = await APICall.Post(customerAddressPostLink, JsonConvert.SerializeObject(PostAddress, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    List<PostCustomer> postCustomer = new List<PostCustomer>();
                                                    PostCustomer customer = new PostCustomer
                                                    {
                                                        origin_application = "OMNI",
                                                        store_sid = storeInfo.SID,
                                                        last_name = BillingAddressInfo.LastName,
                                                        first_name = BillingAddressInfo.FirstName,
                                                        customer_active = 1,
                                                        customer_type = 0,
                                                        full_name = BillingAddressInfo.Name,
                                                        phones = new List<Phone>(),
                                                        address = new List<Address>()
                                                    };

                                                    customer.phones.Add(new Phone
                                                    {
                                                        origin_application = "OMNI",
                                                        phone_no = CustomerPhoneNumber,
                                                        primary_flag = true,
                                                        seq_no = 1,
                                                    });

                                                    var completeAddress = (BillingAddressInfo.Address1 + " " + BillingAddressInfo.Address2).Trim();
                                                    completeAddress = string.IsNullOrEmpty(completeAddress) ? (ShippingAddressInfo.Address1 + " " + ShippingAddressInfo.Address2).Trim() : completeAddress;

                                                    var address_line_1 = completeAddress.Length > 40 ? completeAddress.Substring(0, 40) : completeAddress;
                                                    var address_line_2 = completeAddress.Length > 80 ? completeAddress.Substring(40, 40) : completeAddress.Length > 40 ? completeAddress.Substring(40, completeAddress.Length - 40) : "";
                                                    var address_line_3 = (completeAddress.Length > 120) ? completeAddress.Substring(80, (completeAddress.Length - 80)) : "";

                                                    customer.address.Add(new Address
                                                    {
                                                        origin_application = "OMNI",
                                                        active = true,
                                                        address_allow_contact = false,
                                                        primary_flag = true,
                                                        city = (string.IsNullOrEmpty(BillingAddressInfo.City) ? ShippingAddressInfo.City : BillingAddressInfo.City)?.ToUpper() ?? "",
                                                        address_line_1 = address_line_1,
                                                        address_line_2 = address_line_2,
                                                        address_line_3 = address_line_3,
                                                        address_line_5 = (string.IsNullOrEmpty(BillingAddressInfo.City) ? ShippingAddressInfo.City : BillingAddressInfo.City)?.ToUpper() ?? "",
                                                        seq_no = ++AddressCount,
                                                        Email = customerInfo.Email ?? OrderInfo.Email
                                                    });
                                                    postCustomer.Add(customer);

                                                    string customerPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/customer";
                                                    var customerPostResponse = await APICall.Post(customerPostLink, JsonConvert.SerializeObject(postCustomer, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);

                                                    if (customerPostResponse.StatusCode == HttpStatusCode.Created)
                                                    {
                                                        var customePostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(customerPostResponse.Content);
                                                        customerPostResponseInfo = customePostResponseObject.FirstOrDefault();
                                                    }
                                                }

                                                #endregion ===================================================================================================== Customer

                                                #region ====================================================================================================== Order Posting

                                                var lineItems = OrderInfo.LineItemList.ToList();

                                                var TOTAL_LINE_ITEM = lineItems.Count();
                                                var ORDER_QTY = lineItems.Sum(s => s.Quantity);
                                                List<ItemPostInfo> ItemPostInfo = new List<ItemPostInfo>();
                                                int itemPos = 1;
                                                List<InventoryModel> itemInfoList = new List<InventoryModel>();

                                                foreach (var item in lineItems)
                                                {
                                                    var itemQry = $@"
                                                       SELECT * from (
                                                            select TO_CHAR(I.SID) AS SID, I.ALU as ALU,I.ALU as SKU, I.UPC as UPC, NVL(IQ.QTY,0) AS STORE_OH,
                                                            NVL(P1.PRICE,0) AS PRICE, s.PRICE_LVL FROM RPS.INVN_SBS_ITEM I
                                                            INNER JOIN RPS.SUBSIDIARY SS ON I.SBS_SID = SS.SID
                                                            CROSS JOIN RPS.PRICE_LEVEL S
                                                            LEFT JOIN RPS.INVN_SBS_PRICE P1 ON I.SID = P1.INVN_SBS_ITEM_SID AND I.SBS_SID = P1.SBS_SID AND S.SID = P1.PRICE_LVL_SID
                                                            LEFT JOIN RPS.INVN_SBS_ITEM_QTY IQ ON I.SID = IQ.INVN_SBS_ITEM_SID AND I.SBS_SID = IQ.SBS_SID AND (IQ.SBS_SID,IQ.STORE_SID) IN (
                                                                SELECT SBS_SID,SID FROM RPS.STORE WHERE STORE_NO = {GlobalVariables.RProConfig.OrderStoreNo} )
                                                            WHERE 1=1
                                                            AND I.SBS_SID = S.SBS_SID
                                                            AND SS.SBS_NO = {GlobalVariables.RProConfig.SBS_NO}
                                                            AND I.Active = 1
                                                            AND NVL(I.ORDERABLE, 0) = 1
                                                        ) pivot (
                                                            sum(PRICE) for PRICE_LVL in (3 AS Price_Lvl_3,4 AS Price_Lvl_4,5 AS Price_Lvl_5,6 AS Price_Lvl_6,7 AS Price_Lvl_7)
                                                        )t
                                                        where t.ALU ='{item.Sku}'
                                                ";

                                                    var itemsResult = ADO.ReadAsync<InventoryModel>(itemQry).FirstOrDefault();

                                                    if (itemsResult != null)
                                                    {
                                                        itemInfoList.Add(itemsResult);

                                                        var itemInfo = new ItemPostInfo
                                                        {
                                                            origin_application = "OMNI",
                                                            invn_sbs_item_sid = itemsResult.SID,
                                                            fulfill_store_sid = storeInfo.SID,
                                                            document_sid = "",
                                                            item_type = 1,
                                                            quantity = item.Quantity,
                                                            manual_disc_type = 0,
                                                            manual_disc_value = item.DiscountedUnitPriceAfterAllDiscountsSet.ShopMoney.Amount,
                                                            manual_disc_reason = "GOODWILL"
                                                        };

                                                        /*
                                                            //var itemInfo2 = new ItemPostInfo
                                                            //{
                                                            //    origin_application = "OMNI",
                                                            //    invn_sbs_item_sid = itemsResult.SID,
                                                            //    order_type = 0,
                                                            //    item_type = 1,
                                                            //    price_lvl = 2,
                                                            //    quantity = item.Quantity,
                                                            //    //fulfill_store_sid = storeInfo.SID,
                                                            //    manual_disc_type = 0,
                                                            //    manual_disc_value = item.DiscountedUnitPriceAfterAllDiscountsSet.ShopMoney.Amount,
                                                            //    manual_disc_reason = "GOODWILL"
                                                            //    //origin_application = "OMNI",
                                                            //    //invn_sbs_item_sid = itemsResult.SID,
                                                            //    //item_type = 1,
                                                            //    //quantity = item.Quantity,
                                                            //    //fulfill_store_sid = storeInfo.SID,
                                                            //};
                                                            //if (item.ProductType == "bundle") itemInfo.kit_type = 2;
                                                            //if (item.TotalDiscountSet.ShopMoney.Amount > 0 && item.OriginalUnitPriceSet.ShopMoney.Amount > item.TotalDiscountSet.ShopMoney.Amount)
                                                            //{
                                                            //    itemInfo.manual_disc_type = 0;
                                                            //    itemInfo.manual_disc_value = item.TotalDiscountSet.ShopMoney.Amount;
                                                            //    itemInfo.manual_disc_reason = "GOODWILL";
                                                            //}
                                                        */

                                                        ItemPostInfo.Add(itemInfo);
                                                        itemPos++;
                                                    }
                                                    else
                                                    {
                                                        var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", $"One or more Items not Found. Ref Item SKU : {item.Sku}");
                                                        OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                                        logDocument = new BsonDocument { { "service", "Post Sale Document" }, { "event", "Document Item Post" }, { "document_id", OrderInfo.OrderId }, { "order_no", OrderInfo.Name }, { "message", $"One or more Items not Found. Ref Item SKU : {item.Sku}" }, { "event_time_date", DateTime.Now } };
                                                        await OrderLogCollection.InsertOneAsync(logDocument);

                                                        continue;
                                                    }
                                                }
                                                if (ItemPostInfo.Count > 0)
                                                {
                                                    List<PostDocument> postDocument = new List<PostDocument>();




                                                    PostDocument document = new PostDocument
                                                    {
                                                        ORIGIN_APPLICATION = "OMNI",
                                                        BT_CUID = customerPostResponseInfo.Sid,
                                                        BT_COUNTRY = OrderInfo.BillingAddress.Country,
                                                        BT_ADDRESS_LINE5 = OrderInfo.BillingAddress.City,
                                                        ST_CUID = customerPostResponseInfo.Sid,
                                                        ST_COUNTRY = OrderInfo.ShippingAddress.Country,
                                                        ST_ADDRESS_LINE5 = OrderInfo.ShippingAddress.City,
                                                        SUBSIDIARY_UID = storeInfo.SBS_SID,
                                                        STORE_NO = storeInfo.STORE_NO,
                                                        STORE_UID = storeInfo.SID,
                                                        NOTES_GENERAL = OrderInfo.Name,
                                                        POS_FLAG3 = "Web",
                                                        UDF_STRING1 = OrderInfo.Courier.CourierName,
                                                        UDF_STRING2 = string.Join(",", OrderInfo.PaymentGatewayNames),
                                                        UDF_STRING3 = PaymentGatewayAccountId, // payment gateway ERP ID 
                                                    };

                                                    postDocument.Add(document);

                                                    var documentPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document";
                                                    var documentResponse = await APICall.Post(documentPostLink, JsonConvert.SerializeObject(postDocument, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);

                                                    if (documentResponse.StatusCode == HttpStatusCode.Created)
                                                    {
                                                        string retailProSid = string.Empty;

                                                        var docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(documentResponse.Content);
                                                        var docPostResponseInfo = docPostResponseObject.FirstOrDefault();
                                                        retailProSid = docPostResponseInfo.Sid;
                                                        string itemPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + retailProSid + "/item";

                                                        ItemPostInfo.ForEach(i => i.document_sid = retailProSid);
                                                        var docItemPost = await APICall.Post(itemPostLink, JsonConvert.SerializeObject(ItemPostInfo, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                                        if (docItemPost.StatusCode == HttpStatusCode.Created)
                                                        {
                                                            var shippingInfo = OrderInfo.Fulfillments.SelectMany(f => f.TrackingInfo).ToList();
                                                            decimal shippingAmount = OrderInfo.TotalShippingPriceSet?.ShopMoney?.Amount ?? 0;

                                                            if (shippingAmount > 0)
                                                            {
                                                                var shippingSidQuery = "SELECT TO_CHAR(SID) as SID FROM RPS.SHIP_METHOD WHERE METHOD = 'Other' " +
                                                                                       $" AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                                                                var shippinfoSid = (ADO.ReadAsync<JObject>(shippingSidQuery)?.FirstOrDefault() ?? [])["SID"]?.ToString();

                                                                List<PutShippingInfo> putShippingInfo = new List<PutShippingInfo>();
                                                                putShippingInfo.Add(new PutShippingInfo
                                                                {
                                                                    shipping_amt_manual = shippingAmount.ToString()
                                                                    //ORDER_SHIPPING_AMT_MANUAL = shippingAmount.ToString(),
                                                                });

                                                                var docRowVersionForShipping = await APICall.GetAsync($"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + retailProSid + "?cols=*", GlobalVariables.RetailProAuthSession);
                                                                docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionForShipping.Content);
                                                                docPostResponseInfo = docPostResponseObject.FirstOrDefault();

                                                                var orderShippingPutUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + retailProSid + "?cols=*&filter=row_version,eq," + docPostResponseInfo.RowVersion;
                                                                var documentDiscountResponse = await APICall.PUT(orderShippingPutUrl, JsonConvert.SerializeObject(putShippingInfo, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                                            }
                                                            // TODO: verify and post Shipping Fee to RetailProDocument if ANY

                                                            string docRowVersionLink1 =
                                                                   $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + retailProSid + "?cols=*";
                                                            var docRowVersionResponse1 = await APICall.GetAsync(docRowVersionLink1, GlobalVariables.RetailProAuthSession);
                                                            docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionResponse1.Content);
                                                            docPostResponseInfo = docPostResponseObject.FirstOrDefault();

                                                            var tender1stCallLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" +
                                                            retailProSid + "?filter=row_version,eq," + docPostResponseInfo.RowVersion;

                                                            var tender2stCallLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" +
                                                                retailProSid + "/tender";

                                                            List<TenderRequest2> tenderRequest2 = new List<TenderRequest2>();

                                                            if (OrderInfo.PaymentGatewayNames.Count > 1)
                                                            {
                                                                foreach (var gateway in OrderInfo.PaymentGatewayNames)
                                                                {
                                                                    if (gateway.Contains("COD"))
                                                                    {
                                                                        var takenAmount = (OrderInfo.Transactions.Where(t => t.Gateway.ToLower() == gateway.ToLower()).ToList()).Sum(t => t.AmountSet.ShopMoney.Amount); // calculate taken amount  ITEM SUM + Shipping
                                                                        TenderRequest2 tenderRequest21 = new TenderRequest2
                                                                        {
                                                                            origin_application = "OMNI",
                                                                            tender_type = 3,
                                                                            document_sid = retailProSid,
                                                                            tender_name = "COD",
                                                                            taken = takenAmount.ToString()
                                                                        };
                                                                        tenderRequest2.Add(tenderRequest21);
                                                                    }
                                                                    else
                                                                    {
                                                                        var takenAmount = (OrderInfo.Transactions.Where(t => t.Gateway.ToLower() == gateway.ToLower()).ToList()).Sum(t => t.AmountSet.ShopMoney.Amount); // calculate taken amount  ITEM SUM + Shipping
                                                                        TenderRequest2 tenderRequest21 = new TenderRequest2
                                                                        {
                                                                            origin_application = "OMNI",
                                                                            tender_type = 2,
                                                                            document_sid = retailProSid,
                                                                            tender_name = "Credit Card",
                                                                            taken = takenAmount.ToString()
                                                                        };
                                                                        tenderRequest2.Add(tenderRequest21);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (OrderInfo.PaymentGatewayNames.Any(s => s.Contains("COD")))
                                                                {
                                                                    var takenAmount = OrderInfo.CurrentTotalPriceSet.ShopMoney.Amount; // calculate taken amount  ITEM SUM + Shipping
                                                                    TenderRequest2 tenderRequest21 = new TenderRequest2
                                                                    {
                                                                        origin_application = "OMNI",
                                                                        tender_type = 3,
                                                                        document_sid = retailProSid,
                                                                        tender_name = "COD",
                                                                        taken = takenAmount.ToString()
                                                                    };
                                                                    tenderRequest2.Add(tenderRequest21);
                                                                }
                                                                else
                                                                {
                                                                    TenderRequest2 tenderRequest21 = new TenderRequest2
                                                                    {
                                                                        origin_application = "OMNI",
                                                                        tender_type = 2,
                                                                        document_sid = retailProSid,
                                                                        tender_name = "Credit Card",
                                                                        taken = OrderInfo.CurrentTotalPriceSet.ShopMoney.Amount.ToString()
                                                                    };
                                                                    tenderRequest2.Add(tenderRequest21);
                                                                }
                                                            }

                                                            var tenderToPost = tenderRequest2
                                                            .GroupBy(x => x.tender_name)
                                                            .Select(g => new TenderRequest2
                                                            {
                                                                origin_application = g.First().origin_application,
                                                                tender_type = g.First().tender_type,
                                                                document_sid = g.First().document_sid,
                                                                tender_name = g.Key,
                                                                taken = g.Sum(x => decimal.Parse(x.taken)).ToString()
                                                            })
                                                            .ToList();

                                                            var tenderPayload = JsonConvert.SerializeObject(tenderToPost, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                                                            var tendResponse2 = await APICall.Post(tender2stCallLink, tenderPayload, RetailProRPCToken);

                                                            if (tendResponse2.IsSuccessful)
                                                            {
                                                                string docRowVersionLink =
                                                                        $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + retailProSid + "?cols=*";
                                                                var docRowVersionResponse = await APICall.GetAsync(docRowVersionLink, GlobalVariables.RetailProAuthSession);
                                                                docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionResponse.Content);
                                                                var docPostResponseInfo2 = docPostResponseObject.FirstOrDefault();

                                                                string documentStatusLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" +
                                                                    docPostResponseInfo2.Sid + "?filter=ROW_VERSION,eq," + docPostResponseInfo2.RowVersion;

                                                                List<OrderStatusPut> orderStatusPuts = new List<OrderStatusPut>
                                                        {
                                                            new OrderStatusPut
                                                            {
                                                                STATUS = 4
                                                            }
                                                        };

                                                                var documentStatusResponse = await APICall.PUT(documentStatusLink, JsonConvert.SerializeObject(orderStatusPuts, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);

                                                                if (documentStatusResponse.IsSuccessful)
                                                                {
                                                                    docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(documentStatusResponse.Content);
                                                                    docPostResponseInfo = docPostResponseObject.FirstOrDefault();

                                                                    var updateFilter = "{order_id:" + OrderInfo.OrderId + "}";
                                                                    var update = Builders<BsonDocument>.Update.Set("posted", true).Set("retailProSid", retailProSid).Set("fulfullment_sent", false).Set("status", "fulfilled");
                                                                    OrderCollection.FindOneAndUpdate(filterQuery, update);

                                                                    logDocument = new BsonDocument { { "event", "Document Posted" }, { "document_id", OrderInfo.OrderId },{ "order_no", OrderInfo.Name }, { "message", "Document Posted Sucessfully" }, { "event_time_date", DateTime.Now } };
                                                                    await OrderLogCollection.InsertOneAsync(logDocument);
                                                                }
                                                                else
                                                                {
                                                                    var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", documentStatusResponse.Content).Set("retailProSid", retailProSid);
                                                                    OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                                                    logDocument = new BsonDocument { { "service", "Post Sale Document" }, { "event", "Document Finalize" }, { "document_id", OrderInfo.OrderId }, { "order_no", OrderInfo.Name }, { "message", documentStatusResponse.Content }, { "event_time_date", DateTime.Now } };
                                                                    await OrderLogCollection.InsertOneAsync(logDocument);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", tendResponse2.Content).Set("retailProSid", retailProSid);
                                                                OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                                                logDocument = new BsonDocument { { "service", "Post Sale Document" }, { "event", "Document Tender Post" }, { "document_id", OrderInfo.OrderId }, { "order_no", OrderInfo.Name }, { "message", tendResponse2.Content }, { "event_time_date", DateTime.Now } };
                                                                await OrderLogCollection.InsertOneAsync(logDocument);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", docItemPost.Content).Set("retailProSid", retailProSid);
                                                            OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                                            logDocument = new BsonDocument { { "service", "Post Sale Document" }, { "event", "Document Item Post" }, { "document_id", OrderInfo.OrderId }, { "order_no", OrderInfo.Name }, { "message", docItemPost.Content }, { "event_time_date", DateTime.Now } };
                                                            await OrderLogCollection.InsertOneAsync(logDocument);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", documentResponse.Content);
                                                        OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);
                                                        //  { "service", "Post Sale Document" }
                                                        logDocument = new BsonDocument { { "service", "Post Sale Document" }, { "event", "Document Post" }, { "document_id", OrderInfo.OrderId }, { "order_no", OrderInfo.Name }, { "message", documentResponse.Content }, { "event_time_date", DateTime.Now } };
                                                        await OrderLogCollection.InsertOneAsync(logDocument);
                                                    }
                                                }
                                            }
                                            #endregion ====================================================================================================== Order Posting

                                        }
                                        else
                                        {

                                            var updateFilter = "{order_id:" + OrderInfo.OrderId + "}";
                                            var update = Builders<BsonDocument>.Update.Set("posted", true).Set("retailProSid", ExistingOrderSid.SID).Set("fulfullment_sent", false).Set("status", "fulfilled");
                                            OrderCollection.FindOneAndUpdate(filterQuery, update);

                                        }

                                    }
                                    else
                                    {
                                        GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig.PrismUser, GlobalVariables.RProConfig.PrismPassword, "webclient");
                                    }
                            }
                            else
                            {
                                var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", "payment gateways account ID not found");
                                OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                BsonDocument document = new BsonDocument();
                                document["created_at"] = DateTime.Now;
                                document["service"] = "Shopify Orders";
                                document["order_no"] = OrderInfo.Name;
                                document["exception_message"] = "Error : payment gateways account ID not found ";
                                document["exception_source"] = "Order Processing";
                                document["exception_stack_trace"] = "";
                                await exceptionCollection.InsertOneAsync(document);

                                continue;
                            }
                        }
                      
                        else
                        {

                            var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", "Multiple payment gateways found ");
                            OrderCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                            BsonDocument document = new BsonDocument();
                            document["created_at"] = DateTime.Now;
                            document["service"] = "Shopify Orders";
                            document["order_no"] = OrderInfo.Name;
                            document["exception_message"] = "Error : Multiple payment gateways found ";
                            document["exception_source"] = "Order Processing";
                            document["exception_stack_trace"] = "";
                            await exceptionCollection.InsertOneAsync(document);

                            continue;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["created_at"] = DateTime.Now;
                document["service"] = "Shopify Orders";
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;

                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private async Task<bool> PosRefundInvoice()
        {
            string mongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            //var OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
            //var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");

            var invoiceCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
            var invoiceLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_invoices_log");

            var SaleRefundCollection = mongoDB.GetCollection<BsonDocument>("shopify_refund_orders");
            var SaleRefundLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_refund_orders_log");

            var countryCollection = mongoDB.GetCollection<BsonDocument>("country");

            var retailproInvCollection = mongoDB.GetCollection<BsonDocument>("retailpro_inventory");
            var ShopifyInvCollection = mongoDB.GetCollection<BsonDocument>("Shopify_inventory");
            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {

                //IDbConnection connection = new OracleConnection(GlobalVariables.OracleConnectionString);

                var filterQuery = "{ posted: false, has_error: false }";
                var orderResult = await SaleRefundCollection.Find(filterQuery).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var orderList = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(orderObj)).ToList();

                if (orderList.Any())
                {
                    // TODO: get Order Retuen Model from Graph QL and Do the changes accordingly

                    foreach (var OrderInfo in orderList)
                    {
                    RetryAuthorization:
                        if (!String.IsNullOrEmpty(GlobalVariables.RetailProAuthSession))
                        {
                            if (OrderInfo.PaymentGatewayNames.Count > 1)
                            {
                                OrderInfo.PaymentGatewayNames = OrderInfo.Transactions.Where(t => !t.Status.ToUpper().Contains("FAIL")).Select(t => t.FormattedGateway).ToList();
                            }

                            var RetailProRPCToken = GlobalVariables.RetailProAuthSession;
                            int AddressCount = 0;
                            string BtCountry = "unknown";
                            string StCountry = "unknown";
                            StoreInfo storeInfo = new StoreInfo();

                            store_Sbs_SID OrderStore_info = new store_Sbs_SID();

                            var tender_name = OrderInfo.PaymentGatewayNames.Any(s => s.Contains("COD")) ? "COD" : "Credit Card";

                            var storeSIdQuery = $"select STORE_NO, to_char(sid) as STORE_SID, to_char(sbs_sid) as SBS_SID from RPS.STORE " +
                            $" where store_no = {GlobalVariables.RProConfig.OrderStoreNo}  AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                            OrderStore_info = ADO.ReadAsync<store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new(); //connection.Query<store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new();

                            var storeInfoQuery = $"Select to_char(sid) as SID,STORE_NAME, STORE_CODE, STORE_NO, to_char(ACTIVE_PRICE_LVL_SID) as ACTIVE_PRICE_LVL_SID, to_char(SBS_SID) as SBS_SID " +
                                $" from Rps.Store where store_no = {OrderStore_info.STORE_NO} " +
                                $" AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                            storeInfo = ADO.ReadAsync<StoreInfo>(storeInfoQuery)?.FirstOrDefault() ?? new(); //connection.Query<StoreInfo>(storeInfoQuery)?.FirstOrDefault() ?? new();   //TODO: combine these two Queries

                            if (GlobalVariables.RProConfig.DefaultStoreNo != GlobalVariables.RProConfig.OrderStoreNo)
                            {
                                try
                                {
                                    var rpcCallUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/security/altersession?action=changevalues"; //
                                    var Payload = $@"{{""subsidiarysid"":""{OrderStore_info.SBS_SID}"",""storesid"":""{OrderStore_info.STORE_SID}""}}";// $@"[{{""MethodName"":""ChangeSubStoreMethod"",""Params"":{{""SubsidiarySid"":""{store_info.SBS_SID}"",""StoreSid"":""{store_info.STORE_SID}""}}}}]";
                                    var rpcResult = await APICall.Post(rpcCallUrl, Payload, GlobalVariables.RetailProAuthSession);

                                    if (rpcResult.IsSuccessful)
                                    {
                                        var rpcResultInfo = JsonConvert.DeserializeObject<List<RpcResult>>(rpcResult.Content).FirstOrDefault();
                                        RetailProRPCToken = rpcResultInfo.Token;
                                    }
                                    else if (rpcResult.StatusCode == HttpStatusCode.Unauthorized)
                                    {
                                        await Task.Delay(3000);
                                        GlobalVariables.RetailProAuthSession = string.Empty;
                                        goto RetryAuthorization;
                                    }
                                    else if (rpcResult.Content.Contains("Authorization failed"))
                                    {
                                        await Task.Delay(3000);
                                        GlobalVariables.RetailProAuthSession = string.Empty;
                                        goto RetryAuthorization;
                                    }
                                    else
                                    {
                                        var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", rpcResult.Content);
                                        SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                        BsonDocument document = new BsonDocument();
                                        document["created_at"] = DateTime.Now;
                                        document["service"] = "Shopify Orders";
                                        document["exception_message"] = "Authorization Error : " + rpcResult.Content;
                                        document["exception_source"] = "Retailpro RPC Call";
                                        document["exception_stack_trace"] = "";
                                        await exceptionCollection.InsertOneAsync(document);

                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var update1 = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", ex.Message);
                                    SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update1);

                                    BsonDocument document = new BsonDocument();
                                    document["created_at"] = DateTime.Now;
                                    document["service"] = "Shopify Orders";
                                    document["exception_message"] = "Error : "+ ex.Message;
                                    document["exception_source"] = "Retailpro RPC Call";
                                    document["exception_stack_trace"] = "";
                                    await exceptionCollection.InsertOneAsync(document);

                                    continue;
                                }
                            }

                            var BillingAddressInfo = OrderInfo.BillingAddress;
                            var ShippingAddressInfo = OrderInfo.ShippingAddress;
                            var customerInfo = OrderInfo.Customer;

                            var existingInvoiceResult = (await invoiceCollection.FindAsync("{order_id:" + OrderInfo.OrderId + "}")).ToList();

                            var OrderValidaterQuery = $"Select To_Char(sid) as sid from Rps.Document where NOTES_GENERAL = '{OrderInfo.Name}'" +
                                $"AND subsidiary_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                            var ExistingOrderSid = ADO.ReadAsync<SidNo>(OrderValidaterQuery).FirstOrDefault();

                            if (existingInvoiceResult != null && existingInvoiceResult.Count > 0 && ExistingOrderSid != null)
                            {
                                var existingInvoiceobj = existingInvoiceResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                                var existingInvoice = JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(existingInvoiceobj)).FirstOrDefault();

                                var retailProInvoiceSid = existingInvoice.RetailProSid;

                                if (!string.IsNullOrEmpty(retailProInvoiceSid))
                                {
                                    string? CustomerPhoneNumber = BillingAddressInfo.Phone != null ? BillingAddressInfo.Phone : ShippingAddressInfo.Phone != null ? ShippingAddressInfo.Phone : null;

                                    var CustomerQuery = " select to_char(C.SID) as SID, " +
                                        " to_char(C.CUST_ID) as CUST_ID, " +
                                        " C.FIRST_NAME, " +
                                        " C.LAST_NAME, " +
                                        " P.PHONE_NO " +
                                        " FROM Rps.customer C, " +
                                        " RPS.Customer_Phone P " +
                                        " WHERE C.SID = P.CUST_SID " +
                                        " AND substr(P.PHONE_NO,-9) like substr('%" + CustomerPhoneNumber + "',-9)";

                                    var ExistingCustomer = ADO.ReadAsync<Customer_Address_info>(CustomerQuery)?.FirstOrDefault() ?? new(); //connection.Query<Customer_Address_info>(CustomerQuery).FirstOrDefault();

                                    var lineItems = OrderInfo.Refunds.Select(s => s.RefundLineItems).SelectMany(s => s.Edges).ToList().Select(s => s.Node).ToList(); //.Where(p => p.IsProcessed == false).ToList();

                                    var TOTAL_LINE_ITEM = lineItems.Count();
                                    var ORDER_QTY = lineItems.Sum(s => s.Quantity);
                                    List<ItemPostInfo> ItemPostInfo = new List<ItemPostInfo>();
                                    int itemPos = 1;

                                    //long.TryParse(retailProInvoiceSid, out long saleRefSid);

                                    foreach (var item in lineItems)
                                    {
                                        var itemQry = $@"
                                               SELECT * from (
                                                    select TO_CHAR(I.SID) AS SID, I.ALU as ALU,I.ALU as SKU, I.UPC as UPC, NVL(IQ.QTY,0) AS STORE_OH,
                                                    NVL(P1.PRICE,0) AS PRICE, s.PRICE_LVL FROM RPS.INVN_SBS_ITEM I
                                                    INNER JOIN RPS.SUBSIDIARY SS ON I.SBS_SID = SS.SID
                                                    CROSS JOIN RPS.PRICE_LEVEL S
                                                    LEFT JOIN RPS.INVN_SBS_PRICE P1 ON I.SID = P1.INVN_SBS_ITEM_SID AND I.SBS_SID = P1.SBS_SID AND S.SID = P1.PRICE_LVL_SID
                                                    LEFT JOIN RPS.INVN_SBS_ITEM_QTY IQ ON I.SID = IQ.INVN_SBS_ITEM_SID AND I.SBS_SID = IQ.SBS_SID AND (IQ.SBS_SID,IQ.STORE_SID) IN (
                                                        SELECT SBS_SID,SID FROM RPS.STORE WHERE STORE_NO = {GlobalVariables.RProConfig.OrderStoreNo} AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO}) )
                                                    WHERE 1=1
                                                    AND I.SBS_SID = S.SBS_SID
                                                    AND SS.SBS_NO = {GlobalVariables.RProConfig.SBS_NO}
                                                    AND I.Active = 1
                                                    AND NVL(I.ORDERABLE, 0) = 1
                                                ) pivot (
                                                    sum(PRICE) for PRICE_LVL in (3 AS Price_Lvl_3,4 AS Price_Lvl_4,5 AS Price_Lvl_5,6 AS Price_Lvl_6,7 AS Price_Lvl_7)
                                                )t
                                                where t.ALU ='{item.LineItem.Sku}'
                                        ";

                                        var itemsResult = ADO.ReadAsync<InventoryModel>(itemQry)?.FirstOrDefault() ?? new(); //connection.Query<InventoryModel>(itemQry).FirstOrDefault();

                                        var docItemrefSidQuery = $@"select to_char(sid) as SID from Rps.Document_item where DOC_SID = '{retailProInvoiceSid}' and INVN_SBS_ITEM_SID = '{itemsResult.SID}'";
                                        JObject docRefResponse = RetailPro2_X.BL.ADO.ReadAsync<JObject>(docItemrefSidQuery)?.FirstOrDefault() ?? new();
                                        var docItemrefSid = docRefResponse["SID"]?.ToString() ?? "NA";

                                        var itemInfo = new ItemPostInfo
                                        {
                                            origin_application = "OMNI",
                                            invn_sbs_item_sid = itemsResult.SID,
                                            item_type = 2,
                                            quantity = item.Quantity,
                                            qty_available_for_return = item.Quantity,
                                            fulfill_store_sid = storeInfo.SID,
                                            return_reason = "Unwanted Item",
                                            price_lvl = 2,
                                            manual_disc_type = 0,
                                            manual_disc_value = item.SubtotalSet.ShopMoney.Amount,
                                            manual_disc_reason = "GOODWILL",
                                            returned_item_invoice_sid = docItemrefSid
                                        };

                                        if (item.LineItem.OriginalTotalSet.ShopMoney.Amount > item.LineItem.DiscountedTotalSet.ShopMoney.Amount)
                                        {
                                            itemInfo.manual_disc_type = 0;
                                            itemInfo.manual_disc_value = item.LineItem.DiscountedTotalSet.ShopMoney.Amount;
                                            itemInfo.manual_disc_reason = "GOODWILL";
                                        }

                                        ItemPostInfo.Add(itemInfo);
                                        itemPos++;
                                    }

                                    {
                                        List<PostDocument> postDocument = new List<PostDocument>();

                                        PostDocument document = new PostDocument
                                        {
                                            ORIGIN_APPLICATION = "OMNI",
                                            ref_sale_sid = retailProInvoiceSid,
                                            BT_CUID = ExistingCustomer.SID,
                                            STORE_UID = storeInfo.SID,
                                            POS_FLAG3 = "Web",
                                            NOTES_GENERAL = OrderInfo.Name,
                                            UDF_DATE1 = OrderInfo.CreatedAt.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                                            UDF_STRING1 = existingInvoice.Courier.CourierName,
                                            UDF_STRING2 = string.Join(",", existingInvoice.PaymentGatewayNames),
                                        };

                                        postDocument.Add(document);

                                        var documentPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document";
                                        var documentResponse = await APICall.Post(documentPostLink, JsonConvert.SerializeObject(postDocument, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                        string NewDocumentSid = string.Empty;
                                        if (documentResponse.StatusCode == HttpStatusCode.Created)
                                        {
                                            var docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(documentResponse.Content);
                                            var docPostResponseInfo = docPostResponseObject.FirstOrDefault();
                                            NewDocumentSid = docPostResponseInfo.Sid;

                                            string itemPostLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + NewDocumentSid + "/item";

                                            var docItemPost = await APICall.Post(itemPostLink, JsonConvert.SerializeObject(ItemPostInfo, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                            if (docItemPost.StatusCode == HttpStatusCode.Created)
                                            {
                                                var shippingInfo = OrderInfo.Fulfillments.SelectMany(f => f.TrackingInfo).ToList();

                                                decimal shippingAmount = OrderInfo.TotalShippingPriceSet?.ShopMoney?.Amount ?? 0;

                                                if (shippingAmount > 0)
                                                {
                                                    var shippingSidQuery = $"SELECT TO_CHAR(SID) as SID FROM RPS.SHIP_METHOD WHERE METHOD = 'Other' " +
                                                        $"AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                                                    var shippinfoSid = (ADO.ReadAsync<JObject>(shippingSidQuery)?.FirstOrDefault() ?? [])["SID"]?.ToString(); //connection.Query<string>(shippingSidQuery).FirstOrDefault();

                                                    List<PutShippingInfo> putShippingInfo = new List<PutShippingInfo>();
                                                    putShippingInfo.Add(new PutShippingInfo
                                                    {
                                                        shipping_amt_manual = (shippingAmount * -1).ToString()
                                                        //ORDER_SHIPPING_AMT_MANUAL = shippingAmount.ToString(),
                                                    });

                                                    var docRowVersionForShipping = await APICall.GetAsync($"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + NewDocumentSid + "?cols=*", GlobalVariables.RetailProAuthSession);
                                                    docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionForShipping.Content);
                                                    docPostResponseInfo = docPostResponseObject.FirstOrDefault();

                                                    var orderShippingPutUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + NewDocumentSid + "?cols=*&filter=row_version,eq," + docPostResponseInfo.RowVersion;
                                                    var documentDiscountResponse = await APICall.PUT(orderShippingPutUrl, JsonConvert.SerializeObject(putShippingInfo, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);
                                                }

                                                string docRowVersionLink1 =
                                                      $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/{NewDocumentSid}?cols=*";
                                                var docRowVersionResponse1 = await APICall.GetAsync(docRowVersionLink1, RetailProRPCToken);
                                                docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionResponse1.Content);

                                                docRowVersionResponse1 = await APICall.GetAsync(docRowVersionLink1, RetailProRPCToken);
                                                docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionResponse1.Content);
                                                docPostResponseInfo = docPostResponseObject.FirstOrDefault();

                                                var tender2stCallLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" +
                                                    NewDocumentSid + "/tender";

                                                List<TenderRequest2> tenderRequest2 = new List<TenderRequest2>();

                                                decimal GivenAmount = 0;
                                                if (OrderInfo.PaymentGatewayNames.Any(s => s.Contains("COD")))
                                                {
                                                    var refundedItems = OrderInfo.Refunds.SelectMany(r => r.RefundLineItems.Edges).Select(e => e.Node).ToList().Where(i => i.IsProcessed == false);
                                                    GivenAmount = refundedItems.Sum(g => g.SubtotalSet.ShopMoney.Amount) + (OrderInfo.TotalShippingPriceSet?.ShopMoney?.Amount ?? 0);
                                                }
                                                else
                                                {
                                                    GivenAmount = OrderInfo.TotalRefundedSet.ShopMoney.Amount;
                                                }
                                                var tendername = "";
                                                var tendertype = 0;
                                                if (OrderInfo.PaymentGatewayNames.Any(s => s.Contains("COD")))
                                                {
                                                    tendername = "COD";
                                                    tendertype = 3;
                                                }
                                                else
                                                {
                                                    tendername = "Credit Card";
                                                    tendertype = 2;
                                                }

                                                    TenderRequest2 tenderRequest21 = new TenderRequest2
                                                    {
                                                        origin_application = "OMNI",
                                                        tender_type = tendertype,
                                                        document_sid = NewDocumentSid,
                                                       // tender_name = OrderInfo.PaymentGatewayNames.Any(s => s.Contains("COD")) ? "COD" : "Credit Card",
                                                       tender_name = tendername,
                                                        given = GivenAmount.ToString()
                                                    };
                                                tenderRequest2.Add(tenderRequest21);

                                                var tendResponse2 = await APICall.Post(tender2stCallLink, JsonConvert.SerializeObject(tenderRequest2, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);

                                                if (tendResponse2.IsSuccessful)
                                                {
                                                    string docRowVersionLink =
                                                            $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" + NewDocumentSid + "?cols=*";
                                                    var docRowVersionResponse = await APICall.GetAsync(docRowVersionLink, RetailProRPCToken);
                                                    docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(docRowVersionResponse.Content);
                                                    docPostResponseInfo = docPostResponseObject.FirstOrDefault();

                                                    string documentStatusLink = $"http://{GlobalVariables.RProConfig.ServerAddress}/v1/rest/document/" +
                                                        NewDocumentSid + "?filter=ROW_VERSION,eq," + docPostResponseInfo.RowVersion;

                                                    List<OrderStatusPut> orderStatusPuts = new List<OrderStatusPut>
                                                    {
                                                        new OrderStatusPut
                                                        {
                                                            STATUS = 4
                                                        }
                                                    };
                                                    var documentStatusResponse = await APICall.PUT(documentStatusLink, JsonConvert.SerializeObject(orderStatusPuts, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), RetailProRPCToken);

                                                    if (documentStatusResponse.IsSuccessful)
                                                    {
                                                        // invoiceCollection

                                                        #region Stock return to Source Store

                                                        var AssignedStoreSid = existingInvoice.assigned_store_sid;//existingInvoice["assigned_store_sid"].ToString();

                                                        var storeSidQuery = $@"select to_char(sbs_sid) as sbs_sid, store_name from rps.store where 1=1 AND sid = {AssignedStoreSid}";
                                                        JObject inStoreInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery)?.FirstOrDefault() ?? new();
                                                        var instoreSbsSid = inStoreInfo["SBS_SID"]?.ToString() ?? "NA";
                                                        var inStoreSid = AssignedStoreSid;

                                                        var items = OrderInfo.Refunds.Select(s => s.RefundLineItems).SelectMany(s => s.Edges).ToList().Select(s => s.Node).ToList(); //OrderInfo.LineItemList.ToList();//.Fulfillments.SelectMany(f => f.FulfillmentLineItemList).ToList();

                                                        var slipItemsList = new List<SlipItemData>();
                                                        foreach (var item in items)
                                                        {
                                                            var itemSidQuery = $@"select to_char(sid) as SID from rps.invn_sbs_item where ALU = '{item.LineItem.Sku}' 
                                                                                AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                                                            JObject itemSidInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(itemSidQuery)?.FirstOrDefault() ?? new();
                                                            var itemSid = itemSidInfo["SID"].ToString();

                                                            slipItemsList.Add(new SlipItemData
                                                            {
                                                                originapplication = "RProPrismWeb",
                                                                itemsid = itemSid,
                                                                slipsid = "NA",
                                                                qty = item.Quantity
                                                            });
                                                        }

                                                        var transferSlipUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip";
                                                        var slipPayload = $@"
                                                        {{
                                                            ""data"": [
                                                                {{
                                                                    ""originapplication"": ""RProPrismWeb"",
                                                                    ""status"": 3,
                                                                    ""docreasonsid"": null,
                                                                    ""insbssid"": ""{instoreSbsSid}"",
                                                                    ""instoresid"": ""{inStoreSid}"",
                                                                    ""outsbssid"": ""{OrderStore_info.SBS_SID}"",
                                                                    ""outstoresid"":""{OrderStore_info.STORE_SID}""
                                                                }}
                                                            ]
                                                        }}
                                                        ";
                                                        // Slip Posting

                                                        var slipResponse = await APICall.Post(transferSlipUrl, slipPayload, GlobalVariables.RetailProAuthSession);
                                                        var responseData = JsonConvert.DeserializeObject<AsnResponse>(slipResponse.Content);
                                                        var slipData = responseData.Data.FirstOrDefault();
                                                        var SlipSid = slipData.Sid;

                                                        // Slip Item Posting
                                                        var transferSlitItemUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}/slipitem";
                                                        slipItemsList.ForEach(i => i.slipsid = SlipSid);

                                                        RootObject rootObject = new RootObject { data = slipItemsList };
                                                        var slipItemResponse = await APICall.Post(transferSlitItemUrl, JsonConvert.SerializeObject(rootObject), GlobalVariables.RetailProAuthSession);

                                                        var commentsUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/slipcomment?comments=Shopify Order No : {OrderInfo.Name}&slipsid={SlipSid}";
                                                        var commentsPayload = $@"{{""data"":[{{""originapplication"":""RProPrismWeb"",""slipsid"":""{SlipSid}"",""comments"":""Shopify Order No : {OrderInfo.Name}""}}]}}";
                                                        var commentsPostResponse = await APICall.Post(commentsUrl, commentsPayload, GlobalVariables.RetailProAuthSession);

                                                        // get RowVersion to finalize slip
                                                        var slipRowversionURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}";
                                                        var slipRowversionResponse = await APICall.GetAsync(slipRowversionURL, GlobalVariables.RetailProAuthSession);
                                                        var slipRowversionData = JsonConvert.DeserializeObject<JObject>(slipRowversionResponse.Content ?? "{}");
                                                        int.TryParse(slipRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int rowversion);

                                                        // finalize slip
                                                        var slipFinalizeUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}";
                                                        var slipFinalizePayload = $@"
                                                        {{
                                                            ""data"": [
                                                                {{
                                                                    ""originapplication"": ""RProPrismWeb"",
                                                                    ""rowversion"": {rowversion},
                                                                    ""status"": 4
                                                                }}
                                                            ]
                                                        }}
                                                        ";
                                                        var slipFinalized = await APICall.PUT(slipFinalizeUrl, slipFinalizePayload, GlobalVariables.RetailProAuthSession);

                                                        var slipfinalizedData = JsonConvert.DeserializeObject<JObject>(slipFinalized.Content ?? "{}");
                                                        var voucherSid = slipfinalizedData?["data"]?[0]?["vousid"]?.ToString();
                                                        // vousid
                                                        if (slipFinalized.IsSuccessful)
                                                        {
                                                            var convertAsnToVoucherURL = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving?action=convertasntovoucher";
                                                            var convertVoucherPayload = $@"
                                                            {{
                                                                ""data"": [
                                                                    {{
                                                                        ""clerksid"": ""745052955000115352"",
                                                                        ""asnsidlist"": ""{voucherSid}"",
                                                                        ""doupdatevoucher"": false,
                                                                        ""originapplication"": ""RProPrismWeb""
                                                                    }}
                                                                ]
                                                            }}
                                                            ";
                                                            var convertResponse = await APICall.Post(convertAsnToVoucherURL, convertVoucherPayload, GlobalVariables.RetailProAuthSession);
                                                            var voucherURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving/{voucherSid}";
                                                            var voucherRowversionResponse = await APICall.GetAsync(voucherURL, GlobalVariables.RetailProAuthSession);
                                                            var voucherRowversionData = JsonConvert.DeserializeObject<JObject>(voucherRowversionResponse.Content ?? "{}");
                                                            int.TryParse(voucherRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int vRowversion);

                                                            var voucherFinalizePayload = $@"
                                                            {{
                                                                ""data"": [
                                                                    {{
                                                                        ""rowversion"": {vRowversion},
                                                                        ""status"": 4,
                                                                        ""approvstatus"": 2,
                                                                        ""publishstatus"": 2
                                                                    }}
                                                                ]
                                                            }}
                                                            ";

                                                            var voucherFinalizeResponse = await APICall.PUT(voucherURL, voucherFinalizePayload, GlobalVariables.RetailProAuthSession);

                                                            if (voucherFinalizeResponse.IsSuccessful)
                                                            {
                                                                foreach (var refund in OrderInfo.Refunds)
                                                                {
                                                                    var RefundlineItems = refund.RefundLineItems.Edges.Select(s => s.Node).ToList();

                                                                    foreach (var refundItem in RefundlineItems)
                                                                    {
                                                                        var filter = Builders<BsonDocument>.Filter.Eq("name", OrderInfo.Name);

                                                                        var updateItems = Builders<BsonDocument>.Update
                                                                        .Set("refunds.$[r].refundLineItems.edges.$[e].node.lineItem.isProcessed", true);
                                                                        var options = new UpdateOptions
                                                                        {
                                                                            ArrayFilters = new List<ArrayFilterDefinition>
                                                                                {
                                                                                    // Match the refund object (optional but recommended)
                                                                                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                                                                        new BsonDocument("r.id", refund.Id)
                                                                                    ),

                                                                                    // Match the specific lineItem inside edges.node
                                                                                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                                                                        new BsonDocument("e.node.lineItem.id", refundItem.LineItem.Id)
                                                                                    )
                                                                                }
                                                                        };

                                                                        await SaleRefundCollection.UpdateOneAsync(filter, updateItems, options);

                                                                    }
                                                                }


                                                                docPostResponseObject = JsonConvert.DeserializeObject<List<PostResponseObject>>(documentStatusResponse.Content);
                                                                docPostResponseInfo = docPostResponseObject.FirstOrDefault();
                                                                var updateFilter = "{order_id:" + OrderInfo.OrderId + "}";
                                                                var update = Builders<BsonDocument>.Update.Set("posted", true).Set("retailProSid", NewDocumentSid);
                                                                SaleRefundCollection.FindOneAndUpdate(filterQuery, update);

                                                                var saleUpdate = Builders<BsonDocument>.Update.Set("status", "Refunded").Set("retailProRefundSid", NewDocumentSid);
                                                                invoiceCollection.FindOneAndUpdate(filterQuery, update);

                                                                logDocument = new BsonDocument { { "event", "Document Finalized" }, { "document_id", OrderInfo.OrderId }, { "message", "Document Posted Sucessfully" }, { "event_time_date", DateTime.Now } };
                                                                await SaleRefundLogCollection.InsertOneAsync(logDocument);
                                                            }
                                                            else
                                                            {
                                                                var update2 = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", voucherFinalizeResponse.Content).Set("retailProSid", NewDocumentSid);
                                                                SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update2);

                                                                logDocument = new BsonDocument { { "service", "Post Refund Document" }, { "event", "Document Finalized" }, { "document_id", OrderInfo.OrderId }, { "message", voucherFinalizeResponse.Content }, { "event_time_date", DateTime.Now } };
                                                                await SaleRefundLogCollection.InsertOneAsync(logDocument);

                                                                BsonDocument document1 = new BsonDocument();
                                                                document1["created_at"] = DateTime.Now;
                                                                document1["service"] = "Shopify Refunds";
                                                                document1["exception_message"] = "Voucher Not Generated";
                                                                document1["exception_source"] = "Prism API";
                                                                document1["exception_stack_trace"] = voucherFinalizeResponse.Content;

                                                                await exceptionCollection.InsertOneAsync(document1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var update3 = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", slipFinalized.Content).Set("retailProSid", NewDocumentSid);
                                                            SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update3);

                                                            logDocument = new BsonDocument { { "service", "Post Refund Document" }, { "event", "Document Finalized" }, { "document_id", OrderInfo.OrderId }, { "message", slipFinalized.Content }, { "event_time_date", DateTime.Now } };
                                                            await SaleRefundLogCollection.InsertOneAsync(logDocument);

                                                            BsonDocument document1 = new BsonDocument();
                                                            document1["created_at"] = DateTime.Now;
                                                            document1["service"] = "Shopify Refunds";
                                                            document1["exception_message"] = "ASN Not Generated";
                                                            document1["exception_source"] = "Prism API";
                                                            document1["exception_stack_trace"] = slipFinalized.Content;

                                                            await exceptionCollection.InsertOneAsync(document1);
                                                        }

                                                        #endregion Stock return to Source Store
                                                    }
                                                    else
                                                    {
                                                        var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", tendResponse2.Content).Set("retailProSid", NewDocumentSid);
                                                        SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                                        logDocument = new BsonDocument { { "service", "Post Refund Document" }, { "event", "Document Finalized" }, { "document_id", OrderInfo.OrderId }, { "message", documentStatusResponse.Content }, { "event_time_date", DateTime.Now } };
                                                        await SaleRefundLogCollection.InsertOneAsync(logDocument);
                                                    }
                                                }
                                                else
                                                {
                                                    var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", tendResponse2.Content).Set("retailProSid", NewDocumentSid);
                                                    SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                                    logDocument = new BsonDocument { { "service", "Post Refund Document" }, { "event", "Document Tender Post" }, { "document_id", OrderInfo.OrderId }, { "message", tendResponse2.Content }, { "event_time_date", DateTime.Now } };
                                                    await SaleRefundLogCollection.InsertOneAsync(logDocument);
                                                }
                                            }
                                            else
                                            {
                                                var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", docItemPost.Content).Set("retailProSid", NewDocumentSid);
                                                SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);
                                                logDocument = new BsonDocument { { "service", "Post Refund Document" }, { "event", "Document Item Post" }, { "document_id", OrderInfo.OrderId }, { "message", "Document Items failed to post in RetailPro " + docItemPost.Content }, { "event_time_date", DateTime.Now } };
                                                await SaleRefundLogCollection.InsertOneAsync(logDocument);
                                            }
                                        }
                                        else
                                        {
                                            var update = Builders<BsonDocument>.Update.Set("posted", false).Set("has_error", true).Set("error_message", documentResponse.Content);
                                            SaleRefundCollection.FindOneAndUpdate("{order_id:" + OrderInfo.OrderId + "}", update);

                                            logDocument = new BsonDocument { { "service", "Post Refund Document" }, { "event", "Document Post" }, { "document_id", OrderInfo.OrderId }, { "message", "Document failed to post in RetailPro " + documentResponse.Content }, { "event_time_date", DateTime.Now } };
                                            await SaleRefundLogCollection.InsertOneAsync(logDocument);
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                           
                            GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig.PrismUser, GlobalVariables.RProConfig.PrismPassword, "webclient");
                        
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["created_at"] = DateTime.Now;
                document["service"] = "Shopify Refunds";
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;

                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

        private async Task<bool> PosCancelSaleOrder()
        {
            string mongoConnectionString = GlobalVariables.MongoConnectionString;
            MongoClient mongoDbClient = new MongoClient(mongoConnectionString);
            var mongoDB = mongoDbClient.GetDatabase(GlobalVariables.MongoDatabase);

            var OrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders");
            var OrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_orders_log");

            var CancelledOrderCollection = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders");
            var CancelledOrderLogCollection = mongoDB.GetCollection<BsonDocument>("shopify_cancelled_orders_log");

            var exceptionCollection = mongoDB.GetCollection<BsonDocument>("exception_log");

            try
            {

                var filterQuery = "{ posted: false, has_error: false, isCancelled:false }";
                var orderResult = await CancelledOrderCollection.Find(filterQuery).ToListAsync();
                var orderObj = orderResult.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                var orderList = JsonConvert.DeserializeObject<List<OrderNode>>(JsonConvert.SerializeObject(orderObj))?.ToList() ?? [];

                if (orderList.Any())
                {
                    foreach (var OrderInfo in orderList)
                    {
                        if (!String.IsNullOrEmpty(GlobalVariables.RetailProAuthSession))
                        {
                            var RetailProRPCToken = GlobalVariables.RetailProAuthSession;
                            StoreInfo storeInfo = new StoreInfo();
                            string NewDocumentSid = string.Empty;
                            string retailProSid = string.Empty;

                            try
                            {
                                var orderFilter = Builders<BsonDocument>.Filter.Eq("order_id", OrderInfo.OrderId);
                                var existingDocument = await OrderCollection.Find(orderFilter).ToListAsync();

                                var OrderValidaterQuery = $"Select To_Char(sid) as sid from Rps.Document where NOTES_GENERAL = '{OrderInfo.Name}' " +
                                    $"AND subsidiary_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                                var ExistingOrderSid = ADO.ReadAsync<SidNo>(OrderValidaterQuery).FirstOrDefault();

                                if (existingDocument.Count > 0)
                                {
                                    if (ExistingOrderSid != null)
                                    {
                                        // Order Posted and cannot be cancelled
                                        var update = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", "Order is Already POsted Cannot be Csncelled Need to Refund");
                                        await CancelledOrderCollection.UpdateOneAsync(orderFilter, update);
                                        await OrderCollection.UpdateOneAsync(orderFilter, update);
                                    }
                                    else
                                    {
                                        var existingDocumentObj = existingDocument.ConvertAll(BsonTypeMapper.MapToDotNetValue);
                                        var saleOrder = (JsonConvert.DeserializeObject<List<OrderForListing>>(JsonConvert.SerializeObject(existingDocumentObj))?.ToList()).FirstOrDefault();

                                        if (saleOrder.dispatched)
                                        {
                                            var update = Builders<BsonDocument>.Update.Set("has_error", true).Set("error_message", "Order is Already Dispatched");
                                            await CancelledOrderCollection.UpdateOneAsync(orderFilter, update);
                                            await OrderCollection.UpdateOneAsync(orderFilter, update);
                                        }
                                        else
                                        {
                                            if (saleOrder.accepted_by_store == "" || saleOrder.accepted_by_store == "pending")
                                            {
                                                var update = Builders<BsonDocument>.Update
                                                    .Set("status", "Cancelled")
                                                    .Set("cancelledAt", OrderInfo.CancelledAt.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"))
                                                    .Set("cancelReason", OrderInfo.CancelReason);
                                                await OrderCollection.UpdateOneAsync(orderFilter, update);

                                                var cancleUpdate = Builders<BsonDocument>.Update.Set("posted", true);

                                                var assignEvent = new EventNode
                                                {
                                                    Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                                                    Message = $"Order {OrderInfo.Name} with Id {OrderInfo.OrderId} is Cancelled",
                                                    CreatedAt = DateTime.UtcNow,
                                                    __typename = "OperationalEvent",
                                                    Action = "Order Released",
                                                    SubjectId = "gid://shopify/Order/" + OrderInfo.OrderId,
                                                    SubjectType = "ORDER"
                                                };

                                                var assignEventDoc = assignEvent.ToBsonDocument();
                                                var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                                                _ = await OrderCollection.UpdateOneAsync(orderFilter, shopifyOrderUpdate);

                                                await CancelledOrderCollection.UpdateOneAsync(orderFilter, cancleUpdate);

                                                // set status = cancelled
                                            }
                                            else if (saleOrder.accepted_by_store == "accepted" && saleOrder.stock_transfered)
                                            {
                                                store_Sbs_SID OrderStore_info = new store_Sbs_SID();

                                                var storeSIdQuery = $"select STORE_NO, to_char(sid) as STORE_SID, to_char(sbs_sid) as SBS_SID from RPS.STORE " +
                                                $" where store_no = {GlobalVariables.RProConfig.OrderStoreNo} and sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";
                                                OrderStore_info = ADO.ReadAsync<store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new(); //connection.Query<store_Sbs_SID>(storeSIdQuery)?.FirstOrDefault() ?? new();

                                                //var update = Builders<BsonDocument>.Update
                                                //    .Set("status", "cancelled")
                                                //    .Set("cancelledAt", OrderInfo.CancelledAt.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"))
                                                //    .Set("cancelReason", OrderInfo.CancelReason);
                                                //await OrderCollection.UpdateOneAsync(orderFilter, update);
                                                //var cancleUpdate = Builders<BsonDocument>.Update.Set("posted", true).Set("isCancelled", true);
                                                //await CancelledOrderCollection.UpdateOneAsync(orderFilter, cancleUpdate);

                                                #region Stock return to Source Store

                                                var AssignedStoreSid = saleOrder.assigned_store_sid.ToString();

                                                var storeSidQuery = $@"select to_char(sbs_sid) as sbs_sid, store_name from rps.store where 1=1 AND sid = {AssignedStoreSid} ";
                                                JObject inStoreInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery)?.FirstOrDefault() ?? new();
                                                var instoreSbsSid = inStoreInfo["SBS_SID"]?.ToString() ?? "NA";
                                                var inStoreSid = AssignedStoreSid;

                                                var items = OrderInfo.LineItemList.ToList();//.Fulfillments.SelectMany(f => f.FulfillmentLineItemList).ToList();
                                                if (items.Count == 0) items = OrderInfo.LineItems.Edges.Select(s => s.Node).ToList(); // 
                                                var slipItemsList = new List<SlipItemData>();
                                                foreach (var item in items)
                                                {
                                                    var itemSidQuery = $@"select to_char(sid) as SID from rps.invn_sbs_item where ALU = '{item.Sku}' 
                                                    AND sbs_sid = (select sid from RPS.SUBSIDIARY where sbs_no = {GlobalVariables.RProConfig.SBS_NO})";

                                                    JObject itemSidInfo = RetailPro2_X.BL.ADO.ReadAsync<JObject>(itemSidQuery)?.FirstOrDefault() ?? new();
                                                    var itemSid = itemSidInfo["SID"].ToString();

                                                    slipItemsList.Add(new SlipItemData
                                                    {
                                                        originapplication = "RProPrismWeb",
                                                        itemsid = itemSid,
                                                        slipsid = "NA",
                                                        qty = item.Quantity
                                                    });
                                                }

                                                var transferSlipUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip";
                                                var slipPayload = $@"
                                                        {{
                                                            ""data"": [
                                                                {{
                                                                    ""originapplication"": ""RProPrismWeb"",
                                                                    ""status"": 3,
                                                                    ""docreasonsid"": null,
                                                                    ""insbssid"": ""{instoreSbsSid}"",
                                                                    ""instoresid"": ""{inStoreSid}"",
                                                                    ""outsbssid"": ""{OrderStore_info.SBS_SID}"",
                                                                    ""outstoresid"":""{OrderStore_info.STORE_SID}""
                                                                }}
                                                            ]
                                                        }}
                                                        ";
                                                // Slip Posting

                                                var slipResponse = await APICall.Post(transferSlipUrl, slipPayload, GlobalVariables.RetailProAuthSession);
                                                var responseData = JsonConvert.DeserializeObject<AsnResponse>(slipResponse.Content ?? "{}");
                                                var SlipSid = responseData.Data.Select(d => d.Sid).FirstOrDefault();

                                                // Slip Item Posting
                                                var transferSlitItemUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}/slipitem";
                                                slipItemsList.ForEach(i => i.slipsid = SlipSid);

                                                RootObject rootObject = new RootObject { data = slipItemsList };
                                                var slipItemResponse = await APICall.Post(transferSlitItemUrl, JsonConvert.SerializeObject(rootObject), GlobalVariables.RetailProAuthSession);

                                                var commentsUrl = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/slipcomment?comments=Shopify Order No : {OrderInfo.Name}&slipsid={SlipSid}";
                                                var commentsPayload = $@"{{""data"":[{{""originapplication"":""RProPrismWeb"",""slipsid"":""{SlipSid}"",""comments"":""Shopify Order No : {OrderInfo.Name}""}}]}}";
                                                var commentsPostResponse = await APICall.Post(commentsUrl, commentsPayload, GlobalVariables.RetailProAuthSession);

                                                // get RowVersion to finalize slip
                                                var slipRowversionURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}";
                                                var slipRowversionResponse = await APICall.GetAsync(slipRowversionURL, GlobalVariables.RetailProAuthSession);
                                                var slipRowversionData = JsonConvert.DeserializeObject<JObject>(slipRowversionResponse.Content ?? "{}");
                                                int.TryParse(slipRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int rowversion);

                                                // finalize slip
                                                var slipFinalizeUrl = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/transferslip/{SlipSid}";
                                                var slipFinalizePayload = $@"
                                                        {{
                                                            ""data"": [
                                                                {{
                                                                    ""originapplication"": ""RProPrismWeb"",
                                                                    ""rowversion"": {rowversion},
                                                                    ""status"": 4
                                                                }}
                                                            ]
                                                        }}
                                                        ";
                                                var slipFinalized = await APICall.PUT(slipFinalizeUrl, slipFinalizePayload, GlobalVariables.RetailProAuthSession);

                                                var slipfinalizedData = JsonConvert.DeserializeObject<JObject>(slipFinalized.Content ?? "{}");
                                                var voucherSid = slipfinalizedData?["data"]?[0]?["vousid"]?.ToString();
                                                // vousid
                                                if (slipFinalized.IsSuccessful)
                                                {
                                                    var convertAsnToVoucherURL = $@"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving?action=convertasntovoucher";
                                                    var convertVoucherPayload = $@"
                                                            {{
                                                                ""data"": [
                                                                    {{
                                                                        ""clerksid"": ""745052955000115352"",
                                                                        ""asnsidlist"": ""{voucherSid}"",
                                                                        ""doupdatevoucher"": false,
                                                                        ""originapplication"": ""RProPrismWeb""
                                                                    }}
                                                                ]
                                                            }}
                                                            ";
                                                    var convertResponse = await APICall.Post(convertAsnToVoucherURL, convertVoucherPayload, GlobalVariables.RetailProAuthSession);
                                                    var voucherURL = $"http://{GlobalVariables.RProConfig.ServerAddress}/api/backoffice/receiving/{voucherSid}";
                                                    var voucherRowversionResponse = await APICall.GetAsync(voucherURL, GlobalVariables.RetailProAuthSession);
                                                    var voucherRowversionData = JsonConvert.DeserializeObject<JObject>(voucherRowversionResponse.Content ?? "{}");
                                                    int.TryParse(voucherRowversionData?["data"]?[0]?["rowversion"]?.ToString(), out int vRowversion);

                                                    var voucherFinalizePayload = $@"
                                                            {{
                                                                ""data"": [
                                                                    {{
                                                                        ""rowversion"": {vRowversion},
                                                                        ""status"": 4,
                                                                        ""approvstatus"": 2,
                                                                        ""publishstatus"": 2
                                                                    }}
                                                                ]
                                                            }}
                                                            ";

                                                    var voucherFinalizeResponse = await APICall.PUT(voucherURL, voucherFinalizePayload, GlobalVariables.RetailProAuthSession);

                                                    if (voucherFinalizeResponse.IsSuccessful)
                                                    {
                                                        // isCancelled
                                                        var update = Builders<BsonDocument>.Update
                                                        .Set("status", "Cancelled")
                                                        .Set("cancelledAt", OrderInfo.CancelledAt.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"))
                                                        .Set("cancelReason", OrderInfo.CancelReason)
                                                        .Set("assigned_store_no", "-1")
                                                        .Set("assigned_store_name", "")
                                                        .Set("accepted_by_store", "")
                                                        .Set("is_courier_assigned", false)
                                                        .Set("stock_transfered", false)
                                                        .Set("cancelReason", OrderInfo.CancelReason);
                                                        await OrderCollection.UpdateOneAsync(orderFilter, update);
                                                        var cancleUpdate = Builders<BsonDocument>.Update.Set("posted", true).Set("isCancelled", true);
                                                        await CancelledOrderCollection.UpdateOneAsync(orderFilter, cancleUpdate);


                                                        var assignEvent = new EventNode
                                                        {
                                                            Id = "gid://Omni/OperationalEvent/" + Guid.NewGuid().ToString(),
                                                            Message = $"Order {OrderInfo.Name} with Id {OrderInfo.OrderId} is Cancelled",
                                                            CreatedAt = DateTime.UtcNow,
                                                            __typename = "OperationalEvent",
                                                            Action = "Order Released",
                                                            SubjectId = "gid://shopify/Order/" + OrderInfo.OrderId,
                                                            SubjectType = "ORDER"
                                                        };

                                                        var assignEventDoc = assignEvent.ToBsonDocument();
                                                        var shopifyOrderUpdate = Builders<BsonDocument>.Update.Push("events", assignEventDoc);
                                                        _ = await OrderCollection.UpdateOneAsync(orderFilter, shopifyOrderUpdate);

                                                    }
                                                    else
                                                    {
                                                        var update = Builders<BsonDocument>.Update
                                                        .Set("has_error", true)
                                                        .Set("error_message", "Voucher Not Generated");
                                                        await CancelledOrderCollection.UpdateOneAsync(orderFilter, update);

                                                        BsonDocument document1 = new BsonDocument();
                                                        document1["created_at"] = DateTime.Now;
                                                        document1["service"] = "Shopify Order Cancelled";
                                                        document1["document_no"] = OrderInfo.OrderId;
                                                        document1["exception_message"] = "Voucher Not Generated";
                                                        document1["exception_source"] = "Prism API";
                                                        document1["exception_trace"] = voucherFinalizeResponse.Content;

                                                        await exceptionCollection.InsertOneAsync(document1);
                                                    }
                                                }
                                                else
                                                {
                                                    BsonDocument document1 = new BsonDocument();
                                                    document1["created_at"] = DateTime.Now;
                                                    document1["service"] = "Shopify Order Cancelled";
                                                    document1["document_no"] = OrderInfo.OrderId;
                                                    document1["exception_message"] = "ASN Not Generated";
                                                    document1["exception_source"] = "Prism API";
                                                    document1["exception_trace"] = slipFinalized.Content;
                                                    await exceptionCollection.InsertOneAsync(document1);
                                                }

                                                #endregion Stock return to Source Store

                                                // set status = cancelled and return stock
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                BsonDocument document = new BsonDocument();
                                document["created_at"] = DateTime.Now;
                                document["service"] = "Shopify Order Cancelled";
                                document["document_no"] = OrderInfo.OrderId;
                                document["exception_message"] = ex.Message;
                                document["exception_source"] = ex.Source;
                                document["exception_stack_trace"] = ex.StackTrace;

                                await exceptionCollection.InsertOneAsync(document);
                            }

                        }
                        else
                        {
                            GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig.PrismUser, GlobalVariables.RProConfig.PrismPassword, "webclient");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                BsonDocument document = new BsonDocument();
                document["created_at"] = DateTime.Now;
                document["service"] = "Shopify Order Cancelled";
                document["exception_message"] = ex.Message;
                document["exception_source"] = ex.Source;
                document["exception_stack_trace"] = ex.StackTrace;

                await exceptionCollection.InsertOneAsync(document);
            }

            return true;
        }

    }
}

namespace RetailPro_V22
{
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
        public string STORE_SID { get; set; }
        public string SBS_NO { get; set; }
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
        public string EMAIL_ADDRESS { get; set; }

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

        [JsonProperty("emails")]
        public List<Email> Emails { get; set; }
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

    public partial class Email
    {
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        [JsonProperty("primary_flag")]
        public bool PrimaryFlag { get; set; }

        //[JsonProperty("first_name")]
        //public string FirstName { get; set; }

        //[JsonProperty("last_name")]
        //public string LastName { get; set; }

        [JsonProperty("seq_no")]
        public long SeqNo { get; set; }

        [JsonProperty("email_allow_contact")]
        public bool EmailAllowContact { get; set; }
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

        public decimal? ORDER_FEE_AMT1 { get; set; }
        public string ORDER_FEE_TYPE1_SID { get; set; }

        public decimal? FEE_AMT1 { get; set; }
        public string FEE_TYPE1_SID { get; set; }
        public string UDF_DATE1 { get; set; }
    }

    public partial class PutShippingInfo
    {
        public string ORDER_SHIP_METHOD_SID { get; set; }
        public string ORDER_SHIPPING_AMT_MANUAL { get; set; }
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

    public partial class PostResponceObject
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
        public string UDF_STRING5 { get; set; }
        public decimal? UDF_FLOAT1 { get; set; }
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

    public class SlipItemData
    {
        public string originapplication { get; set; }
        public string itemsid { get; set; }
        public string slipsid { get; set; }
        public long qty { get; set; }
    }

    public class RootObject
    {
        public List<SlipItemData> data { get; set; }
    }

    public partial class AsnResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("metatype")]
        public string Metatype { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("translationid")]
        public string Translationid { get; set; }

        [JsonProperty("errors")]
        public object Errors { get; set; }

        [JsonProperty("data")]
        public List<SlipData> Data { get; set; }
    }

    public partial class SlipData
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
        public object Modifieddatetime { get; set; }

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

        [JsonProperty("outsbssid")]
        public string Outsbssid { get; set; }

        [JsonProperty("outstoresid")]
        public string Outstoresid { get; set; }

        [JsonProperty("insbssid")]
        public string Insbssid { get; set; }

        [JsonProperty("instoresid")]
        public string Instoresid { get; set; }

        [JsonProperty("slipno")]
        public long Slipno { get; set; }

        [JsonProperty("clerksid")]
        public string Clerksid { get; set; }

        [JsonProperty("etadate")]
        public object Etadate { get; set; }

        [JsonProperty("refslipsid")]
        public object Refslipsid { get; set; }

        [JsonProperty("tono")]
        public object Tono { get; set; }

        [JsonProperty("tosid")]
        public object Tosid { get; set; }

        [JsonProperty("vousid")]
        public object Vousid { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("procstatus")]
        public long Procstatus { get; set; }

        [JsonProperty("reversedflag")]
        public object Reversedflag { get; set; }

        [JsonProperty("usevat")]
        public bool Usevat { get; set; }

        [JsonProperty("taxareasid")]
        public string Taxareasid { get; set; }

        [JsonProperty("docpricetotal")]
        public long Docpricetotal { get; set; }

        [JsonProperty("doccosttotal")]
        public long Doccosttotal { get; set; }

        [JsonProperty("doccostsubtotal")]
        public long Doccostsubtotal { get; set; }

        [JsonProperty("origstoresid")]
        public string Origstoresid { get; set; }

        [JsonProperty("origstation")]
        public object Origstation { get; set; }

        [JsonProperty("trackingno")]
        public object Trackingno { get; set; }

        [JsonProperty("shipmentno")]
        public object Shipmentno { get; set; }

        [JsonProperty("daysintran")]
        public object Daysintran { get; set; }

        [JsonProperty("custfld")]
        public object Custfld { get; set; }

        [JsonProperty("station")]
        public object Station { get; set; }

        [JsonProperty("workstation")]
        public long Workstation { get; set; }

        [JsonProperty("wsseqno")]
        public object Wsseqno { get; set; }

        [JsonProperty("verifytype")]
        public object Verifytype { get; set; }

        [JsonProperty("verifydate")]
        public object Verifydate { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("resolvebyid")]
        public object Resolvebyid { get; set; }

        [JsonProperty("resolvedate")]
        public object Resolvedate { get; set; }

        [JsonProperty("resolvemethod")]
        public object Resolvemethod { get; set; }

        [JsonProperty("resolvestatus")]
        public object Resolvestatus { get; set; }

        [JsonProperty("resolvesid")]
        public object Resolvesid { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("controller")]
        public long Controller { get; set; }

        [JsonProperty("origcontroller")]
        public long Origcontroller { get; set; }

        [JsonProperty("createdbysid")]
        public string Createdbysid { get; set; }

        [JsonProperty("modifiedbysid")]
        public string Modifiedbysid { get; set; }

        [JsonProperty("transreasonsid")]
        public object Transreasonsid { get; set; }

        [JsonProperty("docreasonsid")]
        public object Docreasonsid { get; set; }

        [JsonProperty("audited")]
        public bool Audited { get; set; }

        [JsonProperty("auditeddate")]
        public object Auditeddate { get; set; }

        [JsonProperty("cms")]
        public bool Cms { get; set; }

        [JsonProperty("cmspostdate")]
        public DateTimeOffset Cmspostdate { get; set; }

        [JsonProperty("archived")]
        public object Archived { get; set; }

        [JsonProperty("note")]
        public object Note { get; set; }

        [JsonProperty("held")]
        public bool Held { get; set; }

        [JsonProperty("unverified")]
        public bool Unverified { get; set; }

        [JsonProperty("carriername")]
        public object Carriername { get; set; }

        [JsonProperty("totaltransferqty")]
        public long Totaltransferqty { get; set; }

        [JsonProperty("lineitems")]
        public long Lineitems { get; set; }

        [JsonProperty("transferqtydiff")]
        public object Transferqtydiff { get; set; }

        [JsonProperty("itemcostdiff")]
        public object Itemcostdiff { get; set; }

        [JsonProperty("lineitemsdiff")]
        public object Lineitemsdiff { get; set; }

        [JsonProperty("serialdiff")]
        public object Serialdiff { get; set; }

        [JsonProperty("lotdiff")]
        public object Lotdiff { get; set; }

        [JsonProperty("resolvedbysid")]
        public object Resolvedbysid { get; set; }

        [JsonProperty("ditstatus")]
        public object Ditstatus { get; set; }

        [JsonProperty("outsbsno")]
        public long Outsbsno { get; set; }

        [JsonProperty("outstoreno")]
        public long Outstoreno { get; set; }

        [JsonProperty("insbsno")]
        public long Insbsno { get; set; }

        [JsonProperty("instoreno")]
        public long Instoreno { get; set; }

        [JsonProperty("clerkname")]
        public string Clerkname { get; set; }

        [JsonProperty("modifiedbyemplname")]
        public string Modifiedbyemplname { get; set; }

        [JsonProperty("createdbyemplname")]
        public string Createdbyemplname { get; set; }

        [JsonProperty("taxareaname")]
        public string Taxareaname { get; set; }

        [JsonProperty("origstoreno")]
        public long Origstoreno { get; set; }

        [JsonProperty("outstoreaddress1")]
        public object Outstoreaddress1 { get; set; }

        [JsonProperty("outstoreaddress2")]
        public object Outstoreaddress2 { get; set; }

        [JsonProperty("outstoreaddress3")]
        public object Outstoreaddress3 { get; set; }

        [JsonProperty("outstoreaddress4")]
        public object Outstoreaddress4 { get; set; }

        [JsonProperty("outstoreaddress5")]
        public object Outstoreaddress5 { get; set; }

        [JsonProperty("outstorename")]
        public string Outstorename { get; set; }

        [JsonProperty("instorecode")]
        public string Instorecode { get; set; }

        [JsonProperty("outstoreudf1string")]
        public object Outstoreudf1String { get; set; }

        [JsonProperty("outstoreudf2string")]
        public object Outstoreudf2String { get; set; }

        [JsonProperty("outstoreudf3string")]
        public object Outstoreudf3String { get; set; }

        [JsonProperty("outstoreudf4string")]
        public object Outstoreudf4String { get; set; }

        [JsonProperty("outstoreudf5string")]
        public object Outstoreudf5String { get; set; }

        [JsonProperty("outstorezip")]
        public object Outstorezip { get; set; }

        [JsonProperty("instorename")]
        public string Instorename { get; set; }

        [JsonProperty("origstoreaddress1")]
        public object Origstoreaddress1 { get; set; }

        [JsonProperty("origstoreaddress2")]
        public object Origstoreaddress2 { get; set; }

        [JsonProperty("origstoreaddress3")]
        public object Origstoreaddress3 { get; set; }

        [JsonProperty("origstoreaddress4")]
        public object Origstoreaddress4 { get; set; }

        [JsonProperty("origstoreaddress5")]
        public object Origstoreaddress5 { get; set; }

        [JsonProperty("origstorename")]
        public string Origstorename { get; set; }

        [JsonProperty("origstoreudf1string")]
        public object Origstoreudf1String { get; set; }

        [JsonProperty("origstoreudf2string")]
        public object Origstoreudf2String { get; set; }

        [JsonProperty("origstoreudf3string")]
        public object Origstoreudf3String { get; set; }

        [JsonProperty("origstoreudf4string")]
        public object Origstoreudf4String { get; set; }

        [JsonProperty("origstoreudf5string")]
        public object Origstoreudf5String { get; set; }

        [JsonProperty("origstorezip")]
        public object Origstorezip { get; set; }

        [JsonProperty("clerkorigsbsno")]
        public long Clerkorigsbsno { get; set; }

        [JsonProperty("createdbyorigsbsno")]
        public long Createdbyorigsbsno { get; set; }

        [JsonProperty("modifiedbyorigsbsno")]
        public long Modifiedbyorigsbsno { get; set; }

        [JsonProperty("resolvedbyemplname")]
        public object Resolvedbyemplname { get; set; }

        [JsonProperty("resolvedbyorigsbsno")]
        public object Resolvedbyorigsbsno { get; set; }

        [JsonProperty("origintimezone")]
        public string Origintimezone { get; set; }

        [JsonProperty("outstorecode")]
        public string Outstorecode { get; set; }

        [JsonProperty("outstoreaddress6")]
        public object Outstoreaddress6 { get; set; }

        [JsonProperty("origstoreaddress6")]
        public object Origstoreaddress6 { get; set; }

        [JsonProperty("instoreaddress1")]
        public string Instoreaddress1 { get; set; }

        [JsonProperty("instoreaddress2")]
        public string Instoreaddress2 { get; set; }

        [JsonProperty("instoreaddress3")]
        public string Instoreaddress3 { get; set; }

        [JsonProperty("instoreaddress4")]
        public object Instoreaddress4 { get; set; }

        [JsonProperty("instoreaddress5")]
        public object Instoreaddress5 { get; set; }

        [JsonProperty("instoreaddress6")]
        public object Instoreaddress6 { get; set; }

        [JsonProperty("instoreudf1string")]
        public object Instoreudf1String { get; set; }

        [JsonProperty("instoreudf2string")]
        public object Instoreudf2String { get; set; }

        [JsonProperty("instoreudf3string")]
        public object Instoreudf3String { get; set; }

        [JsonProperty("instoreudf4string")]
        public object Instoreudf4String { get; set; }

        [JsonProperty("instoreudf5string")]
        public object Instoreudf5String { get; set; }

        [JsonProperty("voupkgno")]
        public object Voupkgno { get; set; }

        [JsonProperty("voutrackingno")]
        public object Voutrackingno { get; set; }

        [JsonProperty("vouclass")]
        public long Vouclass { get; set; }

        [JsonProperty("instorephone1")]
        public string Instorephone1 { get; set; }

        [JsonProperty("instorephone2")]
        public object Instorephone2 { get; set; }

        [JsonProperty("origstorephone1")]
        public object Origstorephone1 { get; set; }

        [JsonProperty("origstorephone2")]
        public object Origstorephone2 { get; set; }

        [JsonProperty("outstorephone1")]
        public object Outstorephone1 { get; set; }

        [JsonProperty("outstorephone2")]
        public object Outstorephone2 { get; set; }

        [JsonProperty("instorezip")]
        public object Instorezip { get; set; }

        [JsonProperty("glflag")]
        public bool Glflag { get; set; }

        [JsonProperty("alextractdate")]
        public object Alextractdate { get; set; }

        [JsonProperty("alpostdate")]
        public object Alpostdate { get; set; }

        [JsonProperty("taxarea2sid")]
        public object Taxarea2Sid { get; set; }

        [JsonProperty("taxarea2name")]
        public object Taxarea2Name { get; set; }

        [JsonProperty("custom0")]
        public object Custom0 { get; set; }

        [JsonProperty("custom1")]
        public object Custom1 { get; set; }

        [JsonProperty("custom2")]
        public object Custom2 { get; set; }

        [JsonProperty("custom3")]
        public object Custom3 { get; set; }

        [JsonProperty("custom4")]
        public object Custom4 { get; set; }

        [JsonProperty("custom5")]
        public object Custom5 { get; set; }

        [JsonProperty("custom6")]
        public object Custom6 { get; set; }

        [JsonProperty("custom7")]
        public object Custom7 { get; set; }

        [JsonProperty("custom8")]
        public object Custom8 { get; set; }

        [JsonProperty("custom9")]
        public object Custom9 { get; set; }

        [JsonProperty("destsublocid")]
        public object Destsublocid { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class PaymentGatewayInfo
    {
        [BsonElement("payment_gateway_name")]
        [JsonProperty("payment_gateway_name")]
        
        public string PaymentGatewayName { get; set; } = string.Empty;

        [BsonElement("payment_gateway_account_id")]
        [JsonProperty("payment_gateway_account_id")]
       
        public string PaymentGatewayAccountId { get; set; } = string.Empty;
    }
    
}