using SharedNavigation.Models;

namespace SharedNavigation.Data
{
    public interface IUserRepository
    {
        Task<UserProfile?> GetUserByObjectIdAsync(string azureAdObjectId);
        Task<UserProfile> CreateUserAsync(UserProfile userProfile);
        Task<UserProfile> UpdateUserAsync(UserProfile userProfile);
        Task<bool> UserExistsAsync(string azureAdObjectId);
    }
}