namespace WebApplication1.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ConfirmedUsers { get; set; }
        public int UnconfirmedUsers { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; } = new();
    }

    public class UserViewModel
    {
        public ApplicationUser User { get; set; } = new();
        public List<string> Roles { get; set; } = new();
    }

    public class SystemInfoViewModel
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public long WorkingSet { get; set; }
        public int TotalUsers { get; set; }
    }
}