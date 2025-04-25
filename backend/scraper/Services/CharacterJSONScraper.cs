using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using scraper.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Collections.Concurrent;

namespace scraper.Services
{
    /// <summary>
    /// Parses through the JSON exposed at Nexon's undocumented API endpoint for Maplestory Rankings.
    /// </summary>
    public class CharacterJSONScraper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _http;

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
                    Job = GetJobFromId(entry.JobID, entry.JobDetail),
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

        public async Task<IEnumerable<Character>> ScrapeAllCharactersAsync(int maxPages = 50000, int concurrency = 10)
        {
            Console.WriteLine($"Starting Scrape ALL Characters: {maxPages} pages with {concurrency} workers.");

            var characters = new ConcurrentBag<Character>();
            var semaphore = new SemaphoreSlim(concurrency);
            var tasks = new List<Task>();

            List<int> failedPages = new();

            for (int i = 0; i < maxPages; i++)
            {
                await semaphore.WaitAsync();

                var currentIndex = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var url = $"https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=legendary&reboot_index=1&page_index={i}1";

                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        
                        var response = await _http.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Headers:");
                            foreach (var header in response.Headers)
                                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");

                            var body = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Request for page {currentIndex} failed with status {response.StatusCode}:\n{body}");
                            return;
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            Console.WriteLine("Empty response received from Nexon API.");
                            return;
                        }

                        var data = JsonSerializer.Deserialize<RankingsApiResponse>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (data?.Ranks == null || data.Ranks.Count == 0)
                        {
                            Console.WriteLine($"Rankings at page {currentIndex} returned 0 characters.");
                            return;
                        }

                        foreach (var entry in data.Ranks)
                        {
                            characters.Add(new Character
                            {
                                Name = entry.CharacterName!,
                                Level = entry.Level,
                                Job = GetJobFromId(entry.JobID, entry.JobDetail),
                                World = entry.WorldName!,
                                ScrapedAt = DateTime.UtcNow,
                                ImageUrl = entry.CharacterImgURL!
                            });
                        }

                        Console.WriteLine($"Finished scraping page {i}1. Currently tracking {characters.Count} characters.");
                        await Task.Delay(Random.Shared.Next(250, 600));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while scraping:\n{ex.Message}");
                        //failedPages.Add(currentIndex);
                        return;
                    }
                    finally 
                    { 
                        semaphore.Release();
                    }
                }));
            }

            //if (failedPages.Any())
            //{
            //    Console.WriteLine($"Retrying {failedPages.Count} failed pages...");
            //    await RetryScrape(failedPages);
            //}

            await Task.WhenAll(tasks);

            Console.WriteLine($"Finished scraping. Total characters collected: {characters.Count}");
            return characters;
        }

        #endregion Scraping

        #region Utils

        private string GetJobFromId(int jobID, int jobDetail)
        {
            // JobID = Class
            // JobDetail = Class Branch (if applicable)
            switch (jobID)
            {
                // Explorers
                case 0:
                    return "Beginner";
                case 1: // Warriors
                    if      (jobDetail == 12) return "Hero";
                    else if (jobDetail == 22) return "Paladin";
                    else if (jobDetail == 32) return "Dark Knight";
                    else                      return "Warrior";
                case 2: // Magicians
                    if      (jobDetail == 12) return "Fire Poison Archmage";
                    else if (jobDetail == 22) return "Ice Lightning Archmage";
                    else if (jobDetail == 32) return "Bishop";
                    else                      return "Magician";
                case 3: // Archers
                    if      (jobDetail == 12) return "Bowmaster";
                    else if (jobDetail == 22) return "Marksman";
                    else if (jobDetail == 32) return "Pathfinder";
                    else                      return "Archer";
                case 4: // Thiefs
                    if      (jobDetail == 12) return "Night Lord";
                    else if (jobDetail == 22) return "Shadower";
                    else if (jobDetail == 34) return "Blade Master";
                    else                      return "Thief";
                case 5: // Pirates
                    if      (jobDetail == 12) return "Buccaneer";
                    else if (jobDetail == 22) return "Corsair";
                    else if (jobDetail == 32) return "Cannon Master";
                    else                      return "Pirate";
                // Cygnus Knights
                case 10:
                    return "Noblesse";
                case 11:
                    return "Dawn Warrior";
                case 12:
                    return "Blaze Wizard";
                case 13:
                    return "Wind Archer";
                case 14:
                    return "Night Walker";
                case 15:
                    return "Thunder Breaker";
                case 202:
                    return "Mihile";
                // Resistance
                case 30:
                    return "Citizen";
                case 31:
                    return "Demon Slayer";
                case 32:
                    return "Battle Mage";
                case 33:
                    return "Wild Hunter";
                case 35:
                    return "Mechanic";
                case 209:
                    return "Demon Avenger";
                case 208:
                    return "Xenon";
                case 215:
                    return "Blaster";
                // Heroes
                case 20:
                    return "Legend";
                case 21:
                    return "Aran";
                case 22:
                    return "Evan";
                case 23:
                    return "Mercedes";
                case 24:
                    return "Phantom";
                case 203:
                    return "Luminous";
                case 212:
                    return "Shade";
                // Nova
                case 204:
                    return "Kaiser";
                case 205:
                    return "Angelic Buster";
                case 216:
                    return "Cadena";
                case 222:
                    return "Kain";
                // Flora
                case 217:
                    return "Illium";
                case 218:
                    return "Ark";
                case 221:
                    return "Adele";
                case 224:
                    return "Khali";
                // Sengoku
                case 206:
                    return "Hayato";
                case 207:
                    return "Kanna";
                // Anima
                case 223:
                    return "Lara";
                case 220:
                    return "Hoyoung";
                // Other
                case 210:
                    return "Zero";
                case 214:
                    return "Kinesis";
                case 225:
                    return "Lynn";
            }
            return "???";
        }

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
