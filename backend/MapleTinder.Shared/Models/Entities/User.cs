namespace MapleTinder.Shared.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string AuthProvider { get; set; } = null!;

        public string OAuthID { get; set; } = null!;
    }
}
