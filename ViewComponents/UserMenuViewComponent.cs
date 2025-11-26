using Microsoft.AspNetCore.Mvc;
using SharedNavigation.Models;
using SharedNavigation.Services;
using System.Threading.Tasks;

namespace SharedNavigation.ViewComponents
{
    /// <summary>
    /// User Menu View Component with Azure AD support
    /// </summary>
    public class UserMenuViewComponent : ViewComponent
    {
        private readonly IAuthenticationService _authService;

        public UserMenuViewComponent(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = HttpContext.User;
            var userInfo = await _authService.GetCurrentUserAsync(claimsPrincipal);
            return View(userInfo);
        }
    }
}