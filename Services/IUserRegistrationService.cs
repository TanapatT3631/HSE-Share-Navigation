using SharedNavigation.Models;

namespace SharedNavigation.Services
{
    public interface IUserRegistrationService
    {
        Task<RegistrationResult> CheckAndRegisterUserAsync(string azureAdObjectId, string email, string displayName);
        Task<UserProfile?> GetUserProfileAsync(string azureAdObjectId);
        Task<UserProfile> UpdateUserProfileAsync(UserProfile userProfile);
    }
}