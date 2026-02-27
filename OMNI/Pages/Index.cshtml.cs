using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace OMNI.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string FromDate { get; set; }

        [BindProperty]
        public string ToDate { get; set; }

        public async Task<IActionResult> OnGet()
        {
            FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            ToDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            //if (User?.Identity?.IsAuthenticated ?? false)
            //{
            //    if (User.IsInRole("Staff"))
            //    {
            //        return Redirect("/OrderListing");
            //    }
            return Page();
            //}
            //else
            //{
            //    return Redirect("/logout");
            //}
        }
    }
}