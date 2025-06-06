using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleTinder.Shared.Models.Entities
{
    public class UserFavourite
    {
        // Composite PK: (UserId, CharacterId)
        public string UserId { get; set; } = null!; // FK from ApplicationUser

        public int CharacterId { get; set; } // FK from Characters

        public DateTime SeenAt { get; set; }

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public Character Character { get; set; } = null!;
    }
}
