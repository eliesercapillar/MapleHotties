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

        // Weekly Stats
        public int WeeklyLikes { get; set; }
        public int WeeklyNopes { get; set; }
        public int WeeklyFavourites { get; set; }

        // Monthly Stats
        public int MonthlyLikes { get; set; }
        public int MonthlyNopes { get; set; }
        public int MonthlyFavourites { get; set; }


        // All Time Stats
        public int TotalLikes { get; set; }
        public int TotalNopes { get; set; }
        public int TotalFavourites { get; set; }

        // Navigation Properties
        public Character Character { get; set; } = null!;
    }
}
