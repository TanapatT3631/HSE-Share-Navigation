// SharedNavigation/Models/AuthenticationModels.cs
using System.Collections.Generic;
using System.Security.Claims;

namespace SharedNavigation.Models
{
    /// <summary>
    /// User Information Model
    /// </summary>
    public class UserInfo
    {
        public string UserId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
        public bool IsAuthenticated { get; set; }
    }

    /// <summary>
    /// Authentication Configuration
    /// </summary>
    public class AuthenticationConfig
    {
        public string TenantId { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string Instance { get; set; } = "https://login.microsoftonline.com/";
        public string CallbackPath { get; set; } = "/signin-oidc";
        public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";
        public List<string> Scopes { get; set; } = new() { "openid", "profile", "email" };
    }

    /// <summary>
    /// Login/Logout URLs
    /// </summary>
    public class AuthenticationUrls
    {
        public string LoginUrl { get; set; } = "/Account/Login";
        public string LogoutUrl { get; set; } = "/Account/Logout";
        public string AccessDeniedUrl { get; set; } = "/Account/AccessDenied";
        public string ProfileUrl { get; set; } = "/Account/Profile";
    }
}



