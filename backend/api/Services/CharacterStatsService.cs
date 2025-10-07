using api.DTOs;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;
using api.Interfaces;

namespace api.Services
{
    public class CharacterStatsService : ICharacterStatsService
    {
        private readonly MapleTinderDbContext _context;

        public CharacterStatsService(MapleTinderDbContext context)
        {
            _context = context;
        }

        public async Task UpdateCharacterStatsAsync(List<SwipeDTO> swipes)
        {
            if (swipes.Count <= 0) return;

            var characterIds = swipes.Select(s => s.CharacterId).ToList();
            var existingStats = await _context.CharacterStats
                .Where(cs => characterIds.Contains(cs.CharacterId))
                .ToListAsync();

            var statsCache = existingStats.ToDictionary(s => s.CharacterId);

            foreach (var swipe in swipes)
            {
                if (!statsCache.TryGetValue(swipe.CharacterId, out var stats))
                {
                    stats = new CharacterStats
                    {
                        CharacterId = swipe.CharacterId,
                        TotalLikes = 0,
                        TotalNopes = 0,
                        TotalFavourites = 0
                    };
                    _context.CharacterStats.Add(stats);
                    statsCache[swipe.CharacterId] = stats;
                }

                switch (swipe.Status?.ToLower())
                {
                    case "liked":
                        stats.TotalLikes++;
                        break;
                    case "noped":
                        stats.TotalNopes++;
                        break;
                    case "favourited":
                        stats.TotalLikes++; // Favourites also count as liking
                        stats.TotalFavourites++;
                        break;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
