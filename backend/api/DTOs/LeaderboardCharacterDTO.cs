using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class LeaderboardCharacterDTO
    {
        public Character Character { get; set; } = null!;
        public int Count { get; set; }
    }
}
