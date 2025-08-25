namespace OrangeHrmApi.Configuration
{
    public class OrangeHrmSettings
    {
        public string BaseUrl { get; set; } = "https://opensource-demo.orangehrmlive.com";
        public string Username { get; set; } = "Admin";
        public string Password { get; set; } = "admin123";
        public int TimeoutSeconds { get; set; } = 30;
        public bool HeadlessMode { get; set; } = true;
        public bool UseSqlite { get; set; } = true;
        public bool AllowDuplicateEmployees { get; set; } = false;
        public bool CaseSensitiveValidation { get; set; } = false;
    }
}
