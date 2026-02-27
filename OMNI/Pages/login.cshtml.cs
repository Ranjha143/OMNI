using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMNI.Pages.BusinessLayerPages;
using System.Security.Claims;

namespace OMNI.Pages
{
    public class loginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        [BindProperty] 
        public string ErrorMessage { get; private set; } = "";

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {

            UserInfoService userService = new UserInfoService();

            var user = await userService.GetUser(Username);
            if (user != null && Password == user.password)
            {
                var claims = new List<Claim>
                {
                    new (ClaimTypes.Sid, user.Id??"0"),
                    new (ClaimTypes.Name, user.username),
                    new (ClaimTypes.Role, user.role),
                };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);

                HttpContext.Session.SetString("username", user.username);

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Invalid credentials";
            return Page();

        }
    }
}
