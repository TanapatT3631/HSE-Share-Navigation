using SharedNavigation.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SharedNavigation.Services
{
    /// <summary>
    /// Authentication Service Interface
    /// Each project implements this based on their auth provider
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Get current user information
        /// </summary>
        public Task<UserInfo> GetCurrentUserAsync(ClaimsPrincipal user);

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        bool IsInRole(ClaimsPrincipal user, string role);

        /// <summary>
        /// Check if user has specific permission/claim
        /// </summary>
        bool HasClaim(ClaimsPrincipal user, string claimType, string claimValue);

        /// <summary>
        /// Get user's photo URL (from Azure AD or default)
        /// </summary>
        Task<string> GetUserPhotoUrlAsync(string userId);
    }
}