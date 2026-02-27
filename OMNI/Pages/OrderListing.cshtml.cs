using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMNI_Dashboard.ApiControllers;
using System.Text.Json.Serialization;

namespace OMNI.Pages
{
    public class OrderListingModel : PageModel
    {
        public void OnGet()
        {
        }


        public JsonResult OnPost(string order_id, string store_no)
        {


            return new JsonResult(true);
        }
    }
}
