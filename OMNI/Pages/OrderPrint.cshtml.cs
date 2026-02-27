using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using OMNI.ApiCalls;
using OMNI_Dashboard.ApiControllers;
using RestSharp;

namespace OMNI.Pages
{
    public class OrderPrintModel : PageModel
    {
        [BindProperty]
        public OrderForListing order { get; set; } = new();

        [BindProperty]
        public bool noCNFound { get; set; } = false;

        [BindProperty]
        public Object StoreInfo { get; set; }

        public async Task OnGet(string order_id)
        {

            var formParams = new Dictionary<string, string>
            {
                { "order_id", order_id }
            };

            var HostUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var response = await RestApiClient.SendAsync($"{HostUrl}/api/v1", "/order_detail", Method.Get, formParams, null, null, false);
            string? rawJsonString = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(response.Content);

            order = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderForListing>(rawJsonString);
            noCNFound = string.IsNullOrEmpty(order.Courier.CnNumber);

        }
    }
}
