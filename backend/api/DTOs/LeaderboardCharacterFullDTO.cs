using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class LeaderboardCharacterFullDTO
    {
        public Character Character { get; set; } = null!;
        public int TotalLikes { get; set; }
        public int TotalNopes { get; set; }
        public int TotalFavourites { get; set; }
    }
}
