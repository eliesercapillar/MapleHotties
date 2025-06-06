using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class FavouriteCharacterDTO
    {
        public Character Character { get; set; } = null!;
        public DateTime SeenAt { get; set; }
    }
}
