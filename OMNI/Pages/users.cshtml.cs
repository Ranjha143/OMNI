using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OMNI.Pages
{
    public class usersModel : PageModel
    {

        [BindProperty]
        public UserInputModel UserInfo { get; set; } = new();
        public List<Store> Stores { get; set; } = [];
        public async Task OnGetAsync()
        {
            var HostUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var client = new RestClient($"{HostUrl}/api/v1");
            var request = new RestRequest("/store/list", Method.Get);
            var response = await client.ExecuteAsync<JObject>(request);

            Stores = JsonConvert.DeserializeObject<List<Store>>(response?.Content??"[]")?.ToList()??[];
        }
       
    }

    public class UserInputModel
    {
        [Required(ErrorMessage = "UserInfo type is required")]
        public string UserType { get; set; }

        [Required(ErrorMessage = "UserInfo full name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "UserInfo email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a valid store")]
        public string StoreId { get; set; }

        [Required(ErrorMessage = "Login ID is required")]
        public string LoginId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        public string ConfirmPassword { get; set; }
    }

    public class Store
    {
        public string sid { get; set; }
        public string store_name { get; set; }
    }
}