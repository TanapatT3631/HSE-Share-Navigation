namespace SharedNavigation.Configuration
{
    public class DepatmentOprions
    {
         public const string SectionName = "Department";

        public string ConnectionString { get; set; } = string.Empty;
        public string TableDepartment { get; set; } = "BOSCH_DEPARTMENT";
        public string TablePlant { get; set; } = "BOSCH_PLANT";
        public bool EnableMiddleware { get; set; } = true;
        public bool AutoRegisterUsers { get; set; } = true;
        public List<string> ExcludedPaths { get; set; } = new()
        {
            "/Account/Logout",
            "/Account/AccessDenied",
            "/api/",
            "/.well-known/",
            "/health"
        };
    }
}