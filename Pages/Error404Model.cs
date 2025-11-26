using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SharedNavigation.Pages
{
    public class Error404Model : PageModel
    {
        public string RequestPath { get; set; } = "";

        public void OnGet()
        {
            RequestPath = HttpContext.Request.Path;
        }
    }
}