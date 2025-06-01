namespace api.DTOs
{
    public class SwipeDTO
    {
        public int CharacterId { get; set; }
        public string? Status { get; set; }  // "nope" | "love" | "favourite"
        public DateTime SeenAt { get; set; }
    }
}
