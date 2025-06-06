using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleTinder.Shared.Models.Entities
{
    public class UserHistory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!; // FK from ApplicationUser

        public int CharacterId { get; set; } // FK from Character

        public string Status { get; set; } = null!; // "Nope" | "Like" | "Favourite"
        public DateTime SeenAt { get; set; }

        // Navigation Properties
        public Character Character { get; set; } = null!;
    }
}
