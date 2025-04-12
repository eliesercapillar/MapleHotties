using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required] public string? Name { get; set; }
        public string CharacterImageUrl { get; set; } = null!;
    }
}
