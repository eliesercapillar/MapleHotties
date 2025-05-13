using System.ComponentModel.DataAnnotations;

namespace api.Authentication.Models.DTOs
{
    public class RegisterDTO
    {
        [Required] public string Email { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
    }

    public class LoginDTO
    {
        [Required] public string Email { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
    }
}
