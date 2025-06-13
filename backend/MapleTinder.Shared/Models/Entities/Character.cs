using System;
using System.ComponentModel.DataAnnotations;

namespace MapleTinder.Shared.Models.Entities
{
    public class Character
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required int Level { get; set; }
        public required string Job { get; set; }
        public required string World { get; set; }
        public required DateTime ScrapedAt { get; set; }
        public required string ImageUrl { get; set; }

        // Navigation Properties
        public CharacterStats? CharacterStats { get; set; }
    }
}
