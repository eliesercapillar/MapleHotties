using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;


public class CharacterScraperPlaywright
{
    private readonly IServiceProvider _serviceProvider;
    private static Lazy<Task<IBrowser>> _lazyBrowserInstance = new(() => InitializeBrowserOnceAsync());

    public CharacterScraperPlaywright(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #region Playwright Initialization/Disposal

    private static async Task<IBrowser> InitializeBrowserOnceAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browserType = playwright.Chromium;

        bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                        || System.IO.File.Exists("/.dockerenv");

        if (isDocker)
        {
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", "/app/.cache/ms-playwright");
        }

        return await browserType.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--disable-gpu", "--disable-dev-shm-usage", "--disable-setuid-sandbox", "--no-sandbox" }
        });
    }

    public static async Task DisposeBrowserAsync()
    {
        if (_lazyBrowserInstance.IsValueCreated)
        {
            var browser = await _lazyBrowserInstance.Value;
            await browser.CloseAsync();
        }
    }

    #endregion Playwright Initialization/Disposal

    #region Scraping

    public async Task<Character> ScrapeCharacterAsync(string characterName)
    {
        Console.WriteLine("Starting ScrapeCHARACTER");

        Console.WriteLine("Getting Browser Instance");
        var browser = await _lazyBrowserInstance.Value;

        Console.WriteLine("Setting Browser Context");
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        Console.WriteLine("Creating new Browser Page");
        var page = await browser.NewPageAsync();

        Console.WriteLine("Going to URL");
        var url = $"https://www.nexon.com/maplestory/rankings/north-america/overall-ranking/legendary?world_type=both&search_type=character-name&search={characterName}";
        await page.GotoAsync(url);

        Console.WriteLine("Locating Table element in DOM");
        // Wait for the page to load and JavaScript to finish rendering (adjust the selector as needed)
        var tableLocator = page.Locator("table[data-section-name='Contents']");
        await tableLocator.WaitForAsync();

        Console.WriteLine("Retrieving Number of rows in DOM");
        // Check if we found any results
        var rowLocator = tableLocator.Locator("tbody tr");

        if (await rowLocator.CountAsync() == 0)
        {
            throw new Exception($"Character '{characterName}' not found in rankings.");
        }

        Console.WriteLine("Processing Row");
        // Parse the first result (you might want to refine this if there are multiple matches)
        var character = await ParseCharacterFromRow(rowLocator.First);

        await page.CloseAsync();
        return character;
    }

    public async Task<IEnumerable<Character>> ScrapeAllCharactersAsync(int maxPages = 50000, int batchSize = 100)
    {
        Console.WriteLine("Starting ScrapeALL");
        var characters = new List<Character>();

        for (int i = 0; i <= maxPages; i += batchSize)
        {
            var batchTasks = new List<Task<IEnumerable<Character>>>();

            for (int j = 0; j < batchSize && (i + j) <= maxPages; j++)
            {
                batchTasks.Add(ScrapeAllCharactersFromPageAsync(i + j));
            }

            var batchResults = await Task.WhenAll(batchTasks);

            foreach (var chars in batchResults)
            {
                characters.AddRange(chars);
            }

            Console.WriteLine($"Scraped {characters.Count} characters so far.");

            if (characters.Count >= 1000)
            {
                Console.WriteLine($"Saving to {characters.Count} characters to database.");
                await SaveCharactersToDatabase(characters);
                characters.Clear();
            }

            int delay = Random.Shared.Next(1000, 3000);
            await Task.Delay(delay);
        }

        await SaveCharactersToDatabase(characters); // Save any remaining

        Console.WriteLine($"Scraping complete. Total characters scraped: {characters.Count}. With {maxPages} and 10 characters per page, we have a {(characters.Count / maxPages * 10.0) * 100.0} successful scrape rate.");
        return characters;
    }
    
    public async Task<IEnumerable<Character>> ScrapeAllCharactersFromPageAsync(int pageIndex)
    {
        var characters = new List<Character>();

        var browser = await _lazyBrowserInstance.Value;
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();

        try
        {
            string url = $"https://www.nexon.com/maplestory/rankings/north-america/overall-ranking/legendary?world_type=both&page_index={pageIndex}";

            // Set a reasonable timeout for page navigation and elements
            page.SetDefaultTimeout(30000);
            await page.GotoAsync(url);

            var tableLocator = page.Locator("table[data-section-name='Contents']");
            await tableLocator.WaitForAsync();

            var rowsLocator = tableLocator.Locator("tbody tr");
            await rowsLocator.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 2000 }); // Wait 2 seconds for DOM to fully render

            var rowCount = await rowsLocator.CountAsync();
            if (rowCount == 0)
            {
                Console.WriteLine($"No characters found on page {pageIndex}. Ending scrape.");
                return characters;
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
                    // Continue with the next row instead of stopping the entire process
                    Console.WriteLine($"Error parsing row {i} on page {pageIndex}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing page {pageIndex}: {ex.Message}");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
        }
        return characters;
    }

    #endregion Scraping

    #region Utils

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
            ImageUrl = hash,
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
