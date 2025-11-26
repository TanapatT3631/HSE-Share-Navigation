using System.Security.Claims;
using SharedNavigation.Models;

namespace SharedNavigation.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetDepartment(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("custom_department")?.Value ?? 
                   principal.FindFirst("department")?.Value;
        }

        public static string? GetPlant(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("custom_plant")?.Value ?? 
                   principal.FindFirst("plant")?.Value;
        }

        public static PlantCode GetCurrentPlant(this ClaimsPrincipal principal)
        {
            var plantClaim = principal.FindFirst("current_plant")?.Value ?? 
                           principal.FindFirst("custom_plant")?.Value;
            
            if (Enum.TryParse<PlantCode>(plantClaim, out var plant))
            {
                return plant;
            }
            return PlantCode.HmjP; // Default
        }

        public static string GetCurrentPlantDisplay(this ClaimsPrincipal principal)
        {
            return principal.GetCurrentPlant().ToString();
        }

        public static string? GetCustomUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("user_id")?.Value;
        }

        public static string? GetObjectId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ??
                   principal.FindFirst("oid")?.Value;
        }

        public static string? GetDisplayName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ??
                   principal.FindFirst("name")?.Value ??
                   principal.Identity?.Name;
        }

        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ??
                   principal.FindFirst("preferred_username")?.Value ??
                   principal.FindFirst("email")?.Value ??
                   principal.Identity?.Name;
        }
    }
}