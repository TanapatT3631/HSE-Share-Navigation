using Microsoft.Extensions.Logging;
using SharedNavigation.Data;
using SharedNavigation.Models;

namespace SharedNavigation.Services
{
    public class UserRegistrationService: IUserRegistrationService
    {
         private readonly IUserRepository _userRepository;
        private readonly ILogger<UserRegistrationService> _logger;
        public UserRegistrationService(
            IUserRepository userRepository,
            ILogger<UserRegistrationService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        public async Task<RegistrationResult> CheckAndRegisterUserAsync(string azureAdObjectId, string email, string displayName)
        {
            try
            {
                if (string.IsNullOrEmpty(azureAdObjectId))
                {
                    return new RegistrationResult
                    {
                        IsRegistered = false,
                        ErrorMessage = "Azure AD Object ID is required"
                    };
                }

                // Check if user already exists
                var existingUser = await _userRepository.GetUserByObjectIdAsync(azureAdObjectId);

                if (existingUser != null)
                {
                    var updateUser = new UserProfile
                    {
                        Id = existingUser.Id,
                        AzureAdObjectId = azureAdObjectId,
                        Email = email ?? string.Empty,
                        DisplayName = displayName ?? string.Empty,
                        Department = GetDeptFromDisplayName(displayName ?? string.Empty),
                        Plant= GetPlantFromEmail(email ?? string.Empty)
                    };
                    await _userRepository.UpdateUserAsync(updateUser);
                    
                    _logger.LogInformation("User {ObjectId} already exists in database", azureAdObjectId);
                    return new RegistrationResult
                    {
                        IsRegistered = true,
                        WasCreated = false,
                        UserProfile = existingUser
                    };
                }

                // Create new user automatically
                var newUser = new UserProfile
                {
                    AzureAdObjectId = azureAdObjectId,
                    Email = email ?? string.Empty,
                    DisplayName = displayName ?? string.Empty,
                    Department = GetDeptFromDisplayName(displayName ?? string.Empty),
                    Plant= GetPlantFromEmail(email ?? string.Empty)
                };

                var createdUser = await _userRepository.CreateUserAsync(newUser);

                _logger.LogInformation("Auto-registered new user {ObjectId} - {Email}", azureAdObjectId, email);

                return new RegistrationResult
                {
                    IsRegistered = true,
                    WasCreated = true,
                    UserProfile = createdUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking/registering user for {ObjectId}", azureAdObjectId);
                return new RegistrationResult
                {
                    IsRegistered = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        private string GetDeptFromDisplayName(string DisplayName)
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                return "";
            }
            int index = DisplayName.IndexOf('(');

            string Dept = index >= 0 && DisplayName.Length > 2
                ? DisplayName.Substring(index + 1, DisplayName.Length - index - 2)
                : string.Empty;
            
            return Dept;
        }
        private string GetPlantFromEmail(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return "";
            }
            int firstNumberIndex = Email.IndexOfAny("0123456789".ToCharArray());
            int atIndex = Email.IndexOf('@');

            string result = (firstNumberIndex >= 0 && atIndex > firstNumberIndex)
                ? Email.Substring(firstNumberIndex + 1, atIndex - firstNumberIndex - 1)
                : string.Empty;
            string capitalizedString = char.ToUpper(result[0]) + result.Substring(1) + "P";
            return capitalizedString;
        }
        public async Task<UserProfile?> GetUserProfileAsync(string azureAdObjectId)
        {
            return await _userRepository.GetUserByObjectIdAsync(azureAdObjectId);
        }

        public async Task<UserProfile> UpdateUserProfileAsync(UserProfile userProfile)
        {
            return await _userRepository.UpdateUserAsync(userProfile);
        }
    }
}