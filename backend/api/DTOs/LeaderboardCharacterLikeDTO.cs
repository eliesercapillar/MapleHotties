using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class LeaderboardCharacterLikeDTO
    {
        public Character Character { get; set; } = null!;
        public int TotalLikes { get; set; }
    }
}
