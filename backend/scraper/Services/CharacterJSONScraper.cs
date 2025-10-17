using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using scraper.DTOs;
using Microsoft.EntityFrameworkCore;
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

        private static readonly string[] _userAgents =
        [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.2420.81",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 OPR/109.0.0.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 14.4; rv:124.0) Gecko/20100101 Firefox/124.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_4_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4.1 Safari/605.1.15",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_4_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 OPR/109.0.0.0",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Linux i686; rv:124.0) Gecko/20100101 Firefox/124.0",
        ];

        private static readonly string[] _acceptLanguages =
        [
            "en-CA,en-US;q=0.9,en;q=0.8",
            "en-CA,en-US;q=0.7,en;q=0.3",
            "en-US,en;q=0.9"
        ];

        public CharacterJSONScraper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _http = new HttpClient();
        }

        #region Scraping 

        public async Task<Character?> ScrapeCharacterAsync(string characterName)
        {
            try
            {
                var url = $"https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=legendary&reboot_index=1&page_index=1&character_name={characterName}";

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request failed with status {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var parsed = ParseResponse(json)?.FirstOrDefault();
                if (parsed == null)
                {
                    Console.WriteLine($"Character '{characterName}' not found.");
                }

                return parsed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping character '{characterName}': {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Character>> ScrapeAllCharactersAsync(int maxPages = 50000, int concurrency = 50)
        {
            Console.WriteLine($"Starting scrape of {maxPages} pages with concurrency {concurrency}.");

            var characters = new ConcurrentBag<Character>();
            var failedPages = new ConcurrentBag<int>();
            var cookie = await GetNewCookieHeaderAsync();

            using var semaphore = new SemaphoreSlim(concurrency);
            var tasks = new List<Task>();

            for (int pageIndex = 0; pageIndex < maxPages; pageIndex++)
            {
                await semaphore.WaitAsync();
                var currentPage = pageIndex;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await ProcessPageAsync(currentPage, cookie, characters, failedPages);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine($"Finished scraping. Total characters: {characters.Count}, failed pages: {failedPages.Count}");
            return characters;
        }

        #endregion

        #region Utils

        private async Task ProcessPageAsync(int pageIndex, string cookie, ConcurrentBag<Character> characters, ConcurrentBag<int> failedPages)
        {
            try
            {
                var request = BuildRequest(pageIndex, cookie);
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request for page {pageIndex} failed with {response.StatusCode}");
                    failedPages.Add(pageIndex);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var parsed = ParseResponse(json);

                if (parsed == null)
                {
                    failedPages.Add(pageIndex);
                    return;
                }

                foreach (var c in parsed)
                    characters.Add(c);

                Console.WriteLine($"Page {pageIndex} scraped — total so far: {characters.Count}");
                await Task.Delay(Random.Shared.Next(500, 1500));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing page {pageIndex}: {ex.Message}");
                failedPages.Add(pageIndex);
            }
        }

        private IEnumerable<Character>? ParseResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            var data = JsonSerializer.Deserialize<RankingsApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null || data.Ranks == null || data.Ranks.Count == 0) return null;

            return data.Ranks.Select(entry => new Character
            {
                Name = entry.CharacterName!,
                Level = entry.Level,
                Job = GetJobFromId(entry.JobID, entry.JobDetail),
                World = GetWorldFromId(entry.WorldID),
                ScrapedAt = DateTime.UtcNow,
                ImageUrl = entry.CharacterImgURL
            });
        }

        private HttpRequestMessage BuildRequest(int pageIndex, string cookie)
        {
            var url = $"https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=legendary&reboot_index=0&page_index={pageIndex}1";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(_userAgents[Random.Shared.Next(_userAgents.Length)]);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptLanguage.ParseAdd(_acceptLanguages[Random.Shared.Next(_acceptLanguages.Length)]);
            request.Headers.Referrer = new Uri("https://www.nexon.com/maplestory/rankings/north-america/overall/legendary?world_type=both");
            request.Headers.TryAddWithoutValidation("Cookie", cookie);

            return request;
        }

        private async Task<string> GetNewCookieHeaderAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgents[Random.Shared.Next(_userAgents.Length)]);
            client.DefaultRequestHeaders.Referrer = new Uri("https://www.nexon.com/maplestory/rankings/north-america/overall/legendary?world_type=both");

            var response = await client.GetAsync("https://www.nexon.com/maplestory/rankings/north-america/overall/legendary?world_type=both");
            response.EnsureSuccessStatusCode();

            var cookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : Enumerable.Empty<string>();
            return string.Join("; ", cookies.Select(c => c.Split(';')[0]));
        }

        private string GetJobFromId(int jobID, int jobDetail)
        {
            return jobID switch
            {
                0 => "Beginner",
                1 => jobDetail switch
                {
                    12 => "Hero",
                    22 => "Paladin",
                    32 => "Dark Knight",
                    _ => "Warrior"
                },
                2 => jobDetail switch
                {
                    12 => "Fire Poison Archmage",
                    22 => "Ice Lightning Archmage",
                    32 => "Bishop",
                    _ => "Magician"
                },
                3 => jobDetail switch
                {
                    12 => "Bowmaster",
                    22 => "Marksman",
                    32 => "Pathfinder",
                    _ => "Archer"
                },
                4 => jobDetail switch
                {
                    12 => "Night Lord",
                    22 => "Shadower",
                    34 => "Blade Master",
                    _ => "Thief"
                },
                5 => jobDetail switch
                {
                    12 => "Buccaneer",
                    22 => "Corsair",
                    32 => "Cannon Master",
                    _ => "Pirate"
                },
                10 => "Noblesse",
                11 => "Dawn Warrior",
                12 => "Blaze Wizard",
                13 => "Wind Archer",
                14 => "Night Walker",
                15 => "Thunder Breaker",
                202 => "Mihile",
                30 => "Citizen",
                31 => "Demon Slayer",
                32 => "Battle Mage",
                33 => "Wild Hunter",
                35 => "Mechanic",
                209 => "Demon Avenger",
                208 => "Xenon",
                215 => "Blaster",
                20 => "Legend",
                21 => "Aran",
                22 => "Evan",
                23 => "Mercedes",
                24 => "Phantom",
                203 => "Luminous",
                212 => "Shade",
                204 => "Kaiser",
                205 => "Angelic Buster",
                216 => "Cadena",
                222 => "Kain",
                217 => "Illium",
                218 => "Ark",
                221 => "Adele",
                224 => "Khali",
                206 => "Hayato",
                207 => "Kanna",
                223 => "Lara",
                220 => "Hoyoung",
                225 => "Lynn",
                226 => "Mo Xuan",
                210 => "Zero",
                214 => "Kinesis",
                227 => "Sia Astelle",
                _ => "???"
            };
        }

        private string GetWorldFromId(int worldID)
        {
            return worldID switch
            {
                0 => "Beginner",
                1 => "Bera",
                19 => "Scania",
                45 => "Kronos",
                70 => "Hyperion",
                _ => "???"
            };
        }

        #endregion Utils

        #region Database

        public async Task SaveCharacterToDatabase(Character character)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MapleTinderDbContext>();

            var existingCharacter = await dbContext.Characters
                .FirstOrDefaultAsync(c => c.Name == character.Name && c.World == character.World);

            if (existingCharacter != null)
            {
                existingCharacter.Level = character.Level;
                existingCharacter.Job = character.Job;
                existingCharacter.ImageUrl = character.ImageUrl;
                existingCharacter.ScrapedAt = DateTime.UtcNow;
            }
            else
            {
                await dbContext.Characters.AddAsync(character);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task SaveCharactersToDatabase(IEnumerable<Character> characters, int batchSize = 500)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MapleTinderDbContext>();

            var characterList = characters.ToList();

            for (int i = 0; i < characterList.Count; i += batchSize)
            {
                var batch = characterList.Skip(i).Take(batchSize).ToList();
                await ProcessBatchAsync(dbContext, batch);

                if (i % 5000 == 0) GC.Collect();
            }
        }

        private async Task ProcessBatchAsync(MapleTinderDbContext dbContext, List<Character> batch)
        {
            var names = batch.Select(c => c.Name).ToList();
            var worlds = batch.Select(c => c.World).ToList();

            var existing = await dbContext.Characters
                .Where(c => names.Contains(c.Name) && worlds.Contains(c.World))
                .ToListAsync();

            foreach (var character in batch)
            {
                var existingCharacter = existing
                    .FirstOrDefault(c => c.Name == character.Name && c.World == character.World);

                if (existingCharacter != null)
                {
                    existingCharacter.Level = character.Level;
                    existingCharacter.Job = character.Job;
                    existingCharacter.ImageUrl = character.ImageUrl;
                    existingCharacter.ScrapedAt = DateTime.UtcNow;
                }
                else
                {
                    await dbContext.Characters.AddAsync(character);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        #endregion
    }
}
