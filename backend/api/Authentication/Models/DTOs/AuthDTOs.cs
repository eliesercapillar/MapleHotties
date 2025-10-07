using System.ComponentModel.DataAnnotations;

namespace api.Authentication.Models.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email is required")] 
        [EmailAddress(ErrorMessage = "Invalid email format")] 
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#]).{8,}$", ErrorMessage = "Password must contain only uppercase characters, lowercase characters, digits, and special characters")]
        public string Password { get; set; } = null!;
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")] 
        public string Password { get; set; } = null!;
    }

    public class LoginSuccessDTO
    {
        [Required(ErrorMessage = "Token is required")] public string Token { get; set; } = null!;
    }
}
