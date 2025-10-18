namespace api.DTOs
{
    public class SwipeDTO
    {
        public int CharacterId { get; set; }
        public string? Status { get; set; }  // "noped" | "liked" | "favourited"
        public DateTime SeenAt { get; set; }
    }
}
