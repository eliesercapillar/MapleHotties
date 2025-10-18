using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class HistoryCharacterDTO
    {
        public Character Character { get; set; } = null!;
        public string Status { get; set; } = null!; // "noped" | "liked" | "favourited"
        public DateTime SeenAt { get; set; }
    }
}
