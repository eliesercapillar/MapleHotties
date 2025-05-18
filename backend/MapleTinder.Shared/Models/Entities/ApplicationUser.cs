using Microsoft.AspNetCore.Identity;

namespace MapleTinder.Shared.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // For OAuth logins (Google, Discord)
        public string? OAuthID { get; set; } // I no longer think this is necessary.
    }
}