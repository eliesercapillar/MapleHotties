using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class LeaderboardCharacterNopeDTO
    {
        public Character Character { get; set; } = null!;
        public int TotalNopes { get; set; }
    }
}
