using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using scraper.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace scraper.Services
{
    /// <summary>
    /// Parses through the JSON exposed at Nexon's undocumented API endpoint for Maplestory Rankings.
    /// </summary>
    public class CharacterJSONScraper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _http;

        private readonly Dictionary<int, string> jobIdToString = new Dictionary<int, string>() 
        { 

        };

        public CharacterJSONScraper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _http = new HttpClient();
        }

        #region Scraping
        public async Task<Character> ScrapeCharacterAsync(string characterName)
        {
            Console.WriteLine("Starting ScrapeCHARACTER");

            try
            {
                var url = $"https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=legendary&reboot_index=1&page_index=1&character_name={characterName}";

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var reason = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Request failed with status {response.StatusCode}: {reason}");
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new Exception("Empty response received from Nexon API.");
                }

                var data = JsonSerializer.Deserialize<RankingsApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data?.Ranks == null || data.Ranks.Count == 0)
                {
                    throw new Exception($"Character '{characterName}' not found in rankings.");
                }

                var entry = data.Ranks.First();

                return new Character
                {
                    Name = entry.CharacterName!,
                    Level = entry.Level,
                    Job = jobIdToString[entry.JobID],
                    World = entry.WorldName!,
                    ScrapedAt = DateTime.UtcNow,
                    ImageUrl = entry.CharacterImgURL!
                };
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error scraping character '{characterName}': {ex.Message}");
                return null!;
            }
        }

        public async Task<IEnumerable<Character>> ScrapeAllCharactersAsync(int maxPages = 50000)
        {
            Console.WriteLine($"Starting Scrape ALL Characters.\nScraping {maxPages} pages.");

            List<Character> characters = new List<Character>();

            for (int i = 0; i < maxPages; i++)
            {
                try
                {
                    var url = $"https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=legendary&reboot_index=1&page_index={i}1";

                    var response = await _http.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        var reason = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Request failed with status {response.StatusCode}: {reason}");
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        throw new Exception("Empty response received from Nexon API.");
                    }

                    var data = JsonSerializer.Deserialize<RankingsApiResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (data?.Ranks == null || data.Ranks.Count == 0)
                    {
                        throw new Exception($"Rankings API returned null.");
                    }

                    var entry = data.Ranks.First();

                    var chars = data.Ranks.Select(r => new Character
                    {
                        Name = entry.CharacterName!,
                        Level = entry.Level,
                        Job = jobIdToString[entry.JobID],
                        World = entry.WorldName!,
                        ScrapedAt = DateTime.UtcNow,
                        ImageUrl = entry.CharacterImgURL!
                    }).ToList();

                    characters.AddRange(chars);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while scraping: {ex.Message}");
                    return characters;
                }
            }
            return characters;
        }

        #endregion Scraping

        #region Utils

        public async Task SaveCharacterToDatabase(Character character)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MapleTinderDbContext>();

            // Check if character already exists by name and world
            var existingCharacter = await dbContext.Characters
                .FirstOrDefaultAsync(c => c.Name == character.Name && c.World == character.World);

            if (existingCharacter != null)
            {
                // Update existing character
                existingCharacter.Level = character.Level;
                existingCharacter.Job = character.Job;
                existingCharacter.ImageUrl = character.ImageUrl;
                existingCharacter.ScrapedAt = DateTime.UtcNow;
            }
            else
            {
                // Add new character
                await dbContext.Characters.AddAsync(character);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task SaveCharactersToDatabase(IEnumerable<Character> characters, int batchSize = 500)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MapleTinderDbContext>();

            var characterList = characters.ToList();

            // Process in smaller batches for better performance and memory management
            for (int i = 0; i < characterList.Count; i += batchSize)
            {
                var batch = characterList.Skip(i).Take(batchSize).ToList();
                await ProcessBatch(dbContext, batch);

                // Force garbage collection to free up memory
                if (i % 5000 == 0)
                {
                    GC.Collect();
                }
            }
        }

        private async Task ProcessBatch(MapleTinderDbContext dbContext, List<Character> batch)
        {
            // Get all existing characters in this batch by name+world
            var characterNames = batch.Select(c => c.Name).ToList();
            var characterWorlds = batch.Select(c => c.World).ToList();

            var existingCharacters = await dbContext.Characters
                .Where(c => characterNames.Contains(c.Name) && characterWorlds.Contains(c.World))
                .ToListAsync();

            foreach (var character in batch)
            {
                var existingCharacter = existingCharacters
                    .FirstOrDefault(c => c.Name == character.Name && c.World == character.World);

                if (existingCharacter != null)
                {
                    // Update existing character
                    existingCharacter.Level = character.Level;
                    existingCharacter.Job = character.Job;
                    existingCharacter.ImageUrl = character.ImageUrl;
                    existingCharacter.ScrapedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new character
                    await dbContext.Characters.AddAsync(character);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        #endregion Utils
    }
}
