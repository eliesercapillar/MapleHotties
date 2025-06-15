using api.DTOs;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class CharacterStatsService
    {
        private readonly MapleTinderDbContext _context;

        public CharacterStatsService(MapleTinderDbContext context)
        {
            _context = context;
        }

        public async Task UpdateCharacterStats(List<SwipeDTO> swipes)
        {
            var characterIds = swipes.Select(s => s.CharacterId).Distinct().ToList();
            var existingStats = await _context.CharacterStats
                .Where(cs => characterIds.Contains(cs.CharacterId))
                .ToListAsync();

            foreach (var characterId in characterIds)
            {
                var stats = existingStats.FirstOrDefault(s => s.CharacterId == characterId);
                if (stats == null)
                {
                    stats = new CharacterStats
                    {
                        CharacterId = characterId,
                        TotalLikes = 0,
                        TotalNopes = 0,
                        TotalFavourites = 0
                    };
                    _context.CharacterStats.Add(stats);
                }

                var swipe = swipes.First(s => s.CharacterId == characterId);
                switch (swipe.Status?.ToLower())
                {
                    case "like":
                        stats.TotalLikes++;
                        break;
                    case "nope":
                        stats.TotalNopes++;
                        break;
                    case "favourite":
                        stats.TotalFavourites++;
                        break;
                }
            }
        }
    }
}
