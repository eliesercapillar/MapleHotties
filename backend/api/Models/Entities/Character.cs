using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.Entities
{
    public class Character
    {   
        public int Id { get; set; }
        [Required]
        public required string CharacterName { get; set; }
        [Required]
        public required string VisualHash { get; set; } // The hash found in the file names of the sprite images from the Official Maplestory Rankings.
        [Required]
        public required int Level { get; set; }
        [Required]
        public required string Job { get; set; }
        [Required]
        public required string World { get; set; }
        [Required]
        public required DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    }
}
