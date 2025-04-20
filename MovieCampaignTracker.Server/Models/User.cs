namespace MovieCampaignTracker.Server.Models
{
    public class User
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ? Role { get; set; } // ✅ Role is now optional
    }
}