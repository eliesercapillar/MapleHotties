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

        public int UserId { get; set; }

        [ForeignKey("Character")] public int CharacterId { get; set; }

        public string Status { get; set; } = null!; // "Nope" | "Like" | "Favourite"
        public DateTime SeenAt { get; set; }
    }
}
