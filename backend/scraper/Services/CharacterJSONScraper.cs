using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using scraper.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using System;
using Microsoft.Playwright;
using System.Net;

namespace scraper.Services
{
    /// <summary>
    /// Parses through the JSON exposed at Nexon's undocumented API endpoint for Maplestory Rankings.
    /// </summary>
    public class CharacterJSONScraper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _http;

        private string[] userAgents =
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

        private string[] acceptLanguages =
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

        //TODO: Fix ScrapeCharacterAsync to queue requests until 403 has passed.
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

        public async Task<IEnumerable<Character>> ScrapeAllCharactersAsync(int maxPages = 50000, int concurrency = 50)
        {
            Console.WriteLine($"Starting Scrape ALL Characters: {maxPages} pages with {concurrency} workers.");

            var characters = new ConcurrentBag<Character>();
            var failedPages = new ConcurrentBag<int>();
            int savedCharacters = 0;

            var semaphore = new SemaphoreSlim(concurrency);
            var tasks = new List<Task>();

            string currentCookie = await GetNewCookieHeaderAsync();
            var cts = new CancellationTokenSource();

            for (int i = 0; i < maxPages; i++)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    savedCharacters += characters.Count();
                    await SaveCharactersToDatabase(characters);
                    characters.Clear();

                    Console.WriteLine("Cancellation requested. Pausing scraping for 5 minutes to wait out 403");
                    await Task.Delay(TimeSpan.FromMinutes(5));

                    Console.WriteLine($"Continuing scraping. Currently at page {i}1");
                    currentCookie = await GetNewCookieHeaderAsync();

                    cts = new CancellationTokenSource();
                }

                if (i > 0 && i % 100 == 0) currentCookie = await GetNewCookieHeaderAsync();

                await semaphore.WaitAsync();

                // Closure
                int currentIndex = i;
                string cookie = currentCookie;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            Console.WriteLine($"Task for page {currentIndex}1 canceled before starting.");
                            return;
                        }

                        var request = GetAndConfigureNewHttpRequestMessage(currentIndex, cookie);
                        var response = await _http.SendAsync(request);

                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                Console.WriteLine($"403 Forbidden hit on page {currentIndex}1. Cancelling all tasks.");
                                cts.Cancel();
                            }

                            var body = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Request for page {currentIndex}1 failed with status {response.StatusCode}:\n{body}");

                            failedPages.Add(currentIndex);
                            return;
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            Console.WriteLine("Empty response received from Nexon API.");
                            failedPages.Add(currentIndex);
                            return;
                        }

                        var data = JsonSerializer.Deserialize<RankingsApiResponse>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (data?.Ranks == null || data.Ranks.Count == 0)
                        {
                            Console.WriteLine($"Rankings at page {currentIndex}1 returned 0 characters.");
                            failedPages.Add(currentIndex);
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

                        Console.WriteLine($"Finished scraping page {currentIndex}1. Currently tracking {characters.Count} characters.");
                        await Task.Delay(Random.Shared.Next(500, 1500));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while scraping:\n{ex.Message}");
                        failedPages.Add(currentIndex);
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

            Console.WriteLine($"Finished scraping. Total characters collected: {characters.Count}. Total pages failed: {failedPages.Count}");
            return characters;
        }

        #endregion Scraping

        #region Utils

        private async Task<string> GetNewCookieHeaderAsync()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync();
            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://www.nexon.com/maplestory/rankings/north-america/overall/legendary?world_type=heroic");

            // Wait for cookies to set
            await page.Locator("#maplestory").WaitForAsync();

            var cookies = await context.CookiesAsync();

            var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));

            await browser.CloseAsync();
            return cookieHeader;
        }

        private HttpRequestMessage GetAndConfigureNewHttpRequestMessage(int currentIndex, string cookie)
        {
            var url = $"https://www.nexon.com/api/maplestory/no-auth/ranking/v2/na?type=overall&id=legendary&reboot_index=1&page_index={currentIndex}1";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Headers
            request.Headers.UserAgent.ParseAdd(userAgents[Random.Shared.Next(userAgents.Length)]);

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new("application/json"));
            request.Headers.Accept.Add(new("text/plain"));
            request.Headers.Accept.Add(new("*/*"));

            request.Headers.AcceptLanguage.Clear();
            request.Headers.AcceptLanguage.ParseAdd(acceptLanguages[Random.Shared.Next(acceptLanguages.Length)]);

            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br, zstd");

            request.Headers.ConnectionClose = false;

            request.Headers.Referrer = new Uri("https://www.nexon.com/maplestory/rankings/north-america/overall/legendary?world_type=heroic");

            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");

            // ---! Cookie !---
            request.Headers.TryAddWithoutValidation("Cookie", cookie);

            return request;
        }

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
