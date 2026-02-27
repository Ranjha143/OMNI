using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMNI_Dashboard.ApiControllers;

namespace OMNI.Pages
{
    public class ConfigurationsModel : PageModel
    {
        [BindProperty]
        public ConfigurationVM Model { get; set; } = new ConfigurationVM();

        [BindProperty]
        public bool inventoryService { get; set; }

        public async Task<IActionResult> OnGet()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                if (User.IsInRole("Super Admin"))
                    return Page();

                return Redirect("/Index");
            }
            else
            {
                return Redirect("/logout");
            }
            //Model.client_name = "Logo";
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                // Validation failed, return the page with errors
                return Page();
            }

            // Access the form values here
            string clientName = Model.client_name;

            // Redirect or return a result
            return RedirectToPage();
        }
    }
}

