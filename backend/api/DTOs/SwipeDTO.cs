namespace api.DTOs
{
    public class SwipeDTO
    {
        public int CharacterId { get; set; }
        public string? Status { get; set; }  // "Nope" | "Like" | "Favourite"
        public DateTime SeenAt { get; set; }
    }
}
