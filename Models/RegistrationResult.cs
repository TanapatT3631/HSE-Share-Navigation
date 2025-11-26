namespace SharedNavigation.Models
{
     public class RegistrationResult
    {
        public bool IsRegistered { get; set; }
        public bool WasCreated { get; set; }
        public UserProfile? UserProfile { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
