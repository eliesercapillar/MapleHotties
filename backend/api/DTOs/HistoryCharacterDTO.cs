using MapleTinder.Shared.Models.Entities;

namespace api.DTOs
{
    public class HistoryCharacterDTO
    {
        public Character Character { get; set; } = null!;
        public string Status { get; set; } = null!; // "nope" | "love" | "favourite"
        public DateTime SeenAt { get; set; }
    }
}
