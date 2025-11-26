using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedNavigation.Configuration;
using SharedNavigation.Services;
using SharedNavigation.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SharedNavigation.Middleware
{
    public class UserRegistrationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserRegistrationMiddleware> _logger;
        private readonly UserRegistrationOptions _options;

        public UserRegistrationMiddleware(
            RequestDelegate next, 
            ILogger<UserRegistrationMiddleware> logger,
            IOptions<UserRegistrationOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, IUserRegistrationService userRegistrationService)
        {
            // Skip if middleware is disabled
            if (!_options.EnableMiddleware)
            {
                await _next(context);
                return;
            }

            // Skip middleware for excluded paths
            if (ShouldSkipMiddleware(context))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            try
            {
                var objectId = GetUserObjectId(context);
                var email = GetUserEmail(context);
                var displayName = GetUserDisplayName(context);


                if (!string.IsNullOrEmpty(objectId) && _options.AutoRegisterUsers)
                {
                    var result = await userRegistrationService.CheckAndRegisterUserAsync(objectId, email, displayName);
                    
                    if (result.WasCreated)
                    {
                        _logger.LogInformation("Auto-registered user {ObjectId} during middleware execution", objectId);
                    }
                    
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        _logger.LogWarning("Failed to auto-register user {ObjectId}: {Error}", objectId, result.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user registration middleware");
                // Continue processing - don't break the application
            }

            await _next(context);
        }

        private bool ShouldSkipMiddleware(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            
            return _options.ExcludedPaths.Any(excludedPath => 
                path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase));
        }

        private static string? GetUserObjectId(HttpContext context)
        {
            return context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? context.User.FindFirst("oid")?.Value;
        }

        private static string GetUserEmail(HttpContext context)
        {
            return context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                ?? context.User.FindFirst("preferred_username")?.Value
                ?? context.User.FindFirst("email")?.Value
                ?? context.User.Identity?.Name
                ?? string.Empty;
        }

        private static string GetUserDisplayName(HttpContext context)
        {
            return context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
                ?? context.User.FindFirst("name")?.Value
                ?? context.User.Identity?.Name
                ?? string.Empty;
        }
        private static void StoreUserDataInSession(HttpContext context, UserProfile? userProfile)
        {
            if (userProfile != null)
            {
                context.Session.SetString("Department", userProfile.Department ?? string.Empty);
                context.Session.SetString("Plant", userProfile.Plant ?? string.Empty);
                context.Session.SetString("UserId", userProfile.Id.ToString());
                context.Session.SetString("Email", userProfile.Email);
                context.Session.SetString("DisplayName", userProfile.DisplayName);
                
                // Log for verification
                var logger = context.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRegistrationMiddleware>>();
                logger.LogInformation("Stored in session - Department: {Department}, Plant: {Plant}", 
                    userProfile.Department, userProfile.Plant);
            }
        }
        
    }
}