namespace SharedNavigation.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string AzureAdObjectId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Plant { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastedSignInDate { get; set; }
    }
}