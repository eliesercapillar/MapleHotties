using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleTinder.Shared.Models.Entities
{
    public class CharacterStats
    {
        public int CharacterId { get; set; }

        public int TotalLikes { get; set; }
        public int TotalNopes { get; set; }
        public int TotalFavourites { get; set; }

        // Navigation Properties
        public Character Character { get; set; } = null!;
    }
}
