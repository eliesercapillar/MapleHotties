using Microsoft.AspNetCore.Identity;

namespace MapleTinder.Shared.Models.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        // For OAuth logins (Google, Discord)
        public string? OAuthID { get; set; }
    }
}