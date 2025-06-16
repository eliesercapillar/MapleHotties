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

            foreach (var swipe in swipes)
            {
                var stats = existingStats.FirstOrDefault(s => s.CharacterId == swipe.CharacterId);
                if (stats == null)
                {
                    stats = new CharacterStats
                    {
                        CharacterId = swipe.CharacterId,
                        TotalLikes = 0,
                        TotalNopes = 0,
                        TotalFavourites = 0
                    };
                    _context.CharacterStats.Add(stats);
                }

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
