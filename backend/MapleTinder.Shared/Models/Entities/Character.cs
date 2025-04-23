using System;
using System.ComponentModel.DataAnnotations;

namespace MapleTinder.Shared.Models.Entities
{
    public class Character
    {
        public int Id { get; set; }

        [Required] public required string Name { get; set; }

        [Required] public required int Level { get; set; }

        [Required] public required string Job { get; set; }

        [Required] public required string World { get; set; }

        [Required] public required DateTime ScrapedAt { get; set; }

        [Required] public required string ImageUrl { get; set; }
    }
}
