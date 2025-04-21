using System.ComponentModel.DataAnnotations;

namespace MovieCampaignTracker.Shared
{
    public class User
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";
    }
}
