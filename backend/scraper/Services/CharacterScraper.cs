using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;


public class CharacterScraper
{
    private readonly IServiceProvider _serviceProvider;

    public CharacterScraper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #region Scraping

    public async Task<IEnumerable<Character>> ScrapeAllCharactersAsync(int maxPages = 50000)
    {
        var characters = new List<Character>();

        using var playwright = await Playwright.CreateAsync();
        var browserType = playwright.Chromium;

        // Launch browser with specific executable path if we're running in Docker
        var browserTypeLaunchOptions = new BrowserTypeLaunchOptions
        {
            Headless = true,
            // Some performance options to handle large-scale scraping
            Args = new[] { "--disable-gpu", "--disable-dev-shm-usage", "--disable-setuid-sandbox", "--no-sandbox" }
        };

        // Check if we're in Docker by looking for a common Docker environment variable
        // or checking for the existence of a Docker-specific file
        bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                        || System.IO.File.Exists("/.dockerenv");

        if (isDocker)
        {
            // Set environment variables for Playwright in Docker
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", "/app/.cache/ms-playwright");
        }

        await using var browser = await browserType.LaunchAsync(browserTypeLaunchOptions);

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        var page = await context.NewPageAsync();

        // Set a reasonable timeout for page navigation and elements
        page.SetDefaultTimeout(30000);

        Console.WriteLine($"Starting scraping of up to {maxPages} pages...");

        for (int pageIndex = 1; pageIndex <= maxPages; pageIndex++)
        {
            try
            {
                string url = $"https://www.nexon.com/maplestory/rankings/north-america/overall-ranking/legendary?world_type=both&page_index={pageIndex}";
                await page.GotoAsync(url);

                // Wait for the table to be loaded
                var tableLocator = page.Locator("table[data-section-name='Contents']");
                await tableLocator.WaitForAsync();

                // Get all rows in the table
                var rowsLocator = tableLocator.Locator("tbody tr");
                var rowCount = await rowsLocator.CountAsync();

                if (rowCount == 0)
                {
                    Console.WriteLine($"No characters found on page {pageIndex}. Ending scrape.");
                    break;
                }

                Console.WriteLine($"Processing page {pageIndex} with {rowCount} characters");

                // Process each row of character data
                for (int i = 0; i < rowCount; i++)
                {
                    try
                    {
                        var rowLocator = rowsLocator.Nth(i);
                        var character = await ParseCharacterFromRow(rowLocator);
                        characters.Add(character);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing row {i} on page {pageIndex}: {ex.Message}");
                        // Continue with the next row instead of stopping the entire process
                    }
                }

                // Optional: Implement a delay to avoid overloading the server
                await Task.Delay(1000);

                // Optional: Save characters in batches (every 1000 characters or every 100 pages)
                if (characters.Count % 1000 == 0)
                {
                    Console.WriteLine($"Total characters scraped so far: {characters.Count}");
                    // You could save batch to database here if needed
                    // SaveCharactersToDatabase(characters);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing page {pageIndex}: {ex.Message}");
                // Decide whether to continue or break based on the error
                if (ex.Message.Contains("Navigation timeout") || ex.Message.Contains("net::ERR"))
                {
                    // Network error, wait longer and retry
                    await Task.Delay(5000);
                    pageIndex--; // Retry the current page
                    continue;
                }
            }
        }

        await browser.CloseAsync();
        Console.WriteLine($"Scraping complete. Total characters scraped: {characters.Count}");
        return characters;
    }

    public async Task<Character> ScrapeCharacterAsync(string characterName)
    {
        using var playwright = await Playwright.CreateAsync();
        var browserType = playwright.Chromium;

        // Launch browser with specific executable path if we're running in Docker
        var browserTypeLaunchOptions = new BrowserTypeLaunchOptions
        {
            Headless = true
        };

        // Check if we're in Docker by looking for a common Docker environment variable
        // or checking for the existence of a Docker-specific file
        bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                        || System.IO.File.Exists("/.dockerenv");

        if (isDocker)
        {
            // Set environment variables for Playwright in Docker
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", "/app/.cache/ms-playwright");
        }

        await using var browser = await browserType.LaunchAsync(browserTypeLaunchOptions);
        var page = await browser.NewPageAsync();

        var url = $"https://www.nexon.com/maplestory/rankings/north-america/overall-ranking/legendary?world_type=both&search_type=character-name&search={characterName}";
        await page.GotoAsync(url);

        // Wait for the page to load and JavaScript to finish rendering (adjust the selector as needed)
        var tableLocator = page.Locator("table[data-section-name='Contents']");
        await tableLocator.WaitForAsync();

        // Check if we found any results
        var rowLocator = tableLocator.Locator("tbody tr");

        if (await rowLocator.CountAsync() == 0)
        {
            await browser.CloseAsync();
            throw new Exception($"Character '{characterName}' not found in rankings.");
        }

        // Parse the first result (you might want to refine this if there are multiple matches)
        var character = await ParseCharacterFromRow(rowLocator.First);

        await browser.CloseAsync();
        return character;
    }

    private async Task<Character> ParseCharacterFromRow(ILocator row)
    {
        // Extract character name (3rd column)
        var nameLocator = row.Locator("td:nth-child(3)");
        var name = await nameLocator.InnerTextAsync();

        // Extract level (6th column, first div)
        var levelLocator = row.Locator("td:nth-child(6) div:first-child");
        var levelText = await levelLocator.InnerTextAsync();
        var level = int.Parse(levelText);

        // Extract world (4th column)
        var worldLocator = row.Locator("td:nth-child(4) i.ranking-world-icon");
        var worldClass = await worldLocator.GetAttributeAsync("class");
        var world = ExtractWorldFromClass(worldClass);

        // Extract job (5th column)
        var jobLocator = row.Locator("td:nth-child(5) img.job-icon");
        var jobTitle = await jobLocator.GetAttributeAsync("title");

        // Extract character image hash from URL
        var characterImgLocator = row.Locator("td:nth-child(2) img.character");
        var imgSrc = await characterImgLocator.GetAttributeAsync("src");
        var hash = ExtractHashFromImageUrl(imgSrc);

        return new Character
        {
            Name = name,
            Level = level,
            Job = jobTitle,
            World = world,
            Hash = hash,
            ScrapedAt = DateTime.UtcNow
        };
    }

    private string ExtractWorldFromClass(string worldClass)
    {
        // The class is typically "ranking-world-icon kronos" where kronos is the world name
        if (string.IsNullOrEmpty(worldClass)) return "Unknown";

        var parts = worldClass.Split(' ');
        if (parts.Length < 2) return "Unknown";

        // Get the last part which should be the world name and capitalize it
        var worldName = parts[parts.Length - 1];
        return char.ToUpper(worldName[0]) + worldName.Substring(1);
    }

    private string ExtractHashFromImageUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";

        // The URL format is like: "https://msavatar1.nexon.net/Character/HASH.png"
        var match = Regex.Match(url, @"Character\/([^.]+)\.png");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    #endregion Scraping

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
            existingCharacter.Hash = character.Hash;
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
                existingCharacter.Hash = character.Hash;
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
}
