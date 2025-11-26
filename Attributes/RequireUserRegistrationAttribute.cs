using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedNavigation.Services;
using Microsoft.AspNetCore.Http;
using SharedNavigation.Models;
using System.Security.Claims;

namespace SharedNavigation.Attributes
{
   public class RequireUserRegistrationAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                await next();
                return;
            }

            var userRegistrationService = context.HttpContext.RequestServices
                .GetRequiredService<IUserRegistrationService>();

            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<RequireUserRegistrationAttribute>>();

            var objectId = GetUserObjectId(context.HttpContext);
            var email = GetUserEmail(context.HttpContext);
            var displayName = GetUserDisplayName(context.HttpContext);

            if (!string.IsNullOrEmpty(objectId))
            {
                try
                {
                    var result = await userRegistrationService.CheckAndRegisterUserAsync(objectId, email, displayName);
                    
                    if (result.WasCreated)
                    {
                        logger.LogInformation("Auto-registered user {ObjectId} via attribute", objectId);
                        
                        // Store user data in session
                        StoreUserDataInSession(context.HttpContext, result.UserProfile);
                        
                        // Add claims to user principal
                        AddUserClaims(context.HttpContext, result.UserProfile);
                    }
                    else if (result.IsRegistered && result.UserProfile != null)
                    {
                        
                        // Store existing user data in session
                        StoreUserDataInSession(context.HttpContext, result.UserProfile);
                        
                        // Add claims to user principal
                        AddUserClaims(context.HttpContext, result.UserProfile);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in RequireUserRegistrationAttribute for user {ObjectId}", objectId);
                    // Continue execution even if there's an error
                }
            }

            await next();
        }

        private static string? GetUserObjectId(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? context.User.FindFirst("oid")?.Value;
        }

        private static string GetUserEmail(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                ?? context.User.FindFirst("preferred_username")?.Value
                ?? context.User.FindFirst("email")?.Value
                ?? context.User.Identity?.Name
                ?? string.Empty;
        }

        private static string GetUserDisplayName(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
                ?? context.User.FindFirst("name")?.Value
                ?? context.User.Identity?.Name
                ?? string.Empty;
        }

        // METHOD ที่แก้ไขแล้ว: Store user data in session
        private static void StoreUserDataInSession(Microsoft.AspNetCore.Http.HttpContext context, UserProfile? userProfile)
        {
            if (userProfile == null) return;

            try
            {
                // ตรวจสอบว่า Session พร้อมใช้งานหรือไม่
                if (context.Session == null)
                {
                    return;
                }

                // Store data in session with null checks
                context.Session.SetString("SelectedPlant_Id", userProfile.Plant ?? "HmjP");
                context.Session.SetString("Plant", userProfile.Plant ?? string.Empty);
                context.Session.SetString("Department", userProfile.Department ?? string.Empty);
                context.Session.SetString("UserId", userProfile.Id.ToString());
                context.Session.SetString("Email", userProfile.Email ?? string.Empty);
                context.Session.SetString("DisplayName", userProfile.DisplayName ?? string.Empty);
                context.Session.SetString("AzureAdObjectId", userProfile.AzureAdObjectId ?? string.Empty);

                var logger = context.RequestServices.GetRequiredService<ILogger<RequireUserRegistrationAttribute>>();
                logger.LogInformation("Stored user data in session for {ObjectId} - Department: {Department}, Plant: {Plant}", 
                    userProfile.AzureAdObjectId, userProfile.Department, userProfile.Plant);
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<RequireUserRegistrationAttribute>>();
                logger.LogError(ex, "Error storing user data in session for {ObjectId}", userProfile.AzureAdObjectId);
            }
        }

        // METHOD ใหม่: Add claims to user principal
        private static void AddUserClaims(Microsoft.AspNetCore.Http.HttpContext context, UserProfile? userProfile)
        {
            if (userProfile == null) return;

            try
            {
                // ตรวจสอบว่ามี custom claims แล้วหรือไม่
                if (context.User.HasClaim(a=>a.Type.Equals("custom_department")) && context.User.HasClaim(a=>a.Type.Equals("custom_plant")))
                {
                    return; // มีแล้วไม่ต้องเพิ่มซ้ำ
                }

                var identity = (ClaimsIdentity)context.User.Identity!;
                var claims = new List<Claim>();

                // เพิ่ม Department และ Plant claims
                if (!string.IsNullOrEmpty(userProfile.Department))
                {
                    // Remove existing claims if any
                    var existingDeptClaims = identity.FindAll("custom_department").ToList();
                    foreach (var claim in existingDeptClaims)
                    {
                        identity.RemoveClaim(claim);
                    }

                    claims.Add(new Claim("custom_department", userProfile.Department));
                    claims.Add(new Claim("department", userProfile.Department));
                }

                if (!string.IsNullOrEmpty(userProfile.Plant))
                {
                    // Remove existing claims if any
                    var existingPlantClaims = identity.FindAll("custom_plant").ToList();
                    foreach (var claim in existingPlantClaims)
                    {
                        identity.RemoveClaim(claim);
                    }

                    claims.Add(new Claim("custom_plant", userProfile.Plant));
                    claims.Add(new Claim("plant", userProfile.Plant));
                }

                // เพิ่ม User ID claim
                if (!context.User.HasClaim(a=>a.Type.Equals("user_id")))
                {
                    claims.Add(new Claim("user_id", userProfile.Id.ToString()));
                }

                // เพิ่ม claims ใหม่
                if (claims.Count > 0)
                {
                    identity.AddClaims(claims);

                    var logger = context.RequestServices.GetRequiredService<ILogger<RequireUserRegistrationAttribute>>();
                    logger.LogInformation("Added claims for user {ObjectId} - Department: {Department}, Plant: {Plant}", 
                        userProfile.AzureAdObjectId, userProfile.Department, userProfile.Plant);
                }
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<RequireUserRegistrationAttribute>>();
                logger.LogError(ex, "Error adding user claims for {ObjectId}", userProfile.AzureAdObjectId);
            }
        }
    }
}