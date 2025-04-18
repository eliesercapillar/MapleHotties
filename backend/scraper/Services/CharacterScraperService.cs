namespace scraper.Services
{
    public class CharacterScraperService : BackgroundService
    {
        private readonly ILogger<CharacterScraperService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromDays(7); // Run once a week
        private int _maxPagesToScrape = 50000; // Default value

        public CharacterScraperService(ILogger<CharacterScraperService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Character Scraper Service is starting.");

            // Initial delay before first run to allow application to fully start
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            using var timer = new PeriodicTimer(_period);

            // Initial run
            await DoWorkAsync(stoppingToken);

            // Continue running on schedule
            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                await DoWorkAsync(stoppingToken);
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Character Scraper Service is running at: {time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scraper = scope.ServiceProvider.GetRequiredService<CharacterScraper>();

                _logger.LogInformation("Starting scheduled character scraping. Max pages: {maxPages}", _maxPagesToScrape);

                var characters = await scraper.ScrapeAllCharactersAsync(_maxPagesToScrape);
                await scraper.SaveCharactersToDatabase(characters);

                _logger.LogInformation("Scheduled character scraping completed. Characters processed: {count}",
                    characters is ICollection<api.Models.Entities.Character> collection ? collection.Count : "unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during scheduled character scraping");
            }
        }

        // Method to allow dynamic configuration changes
        public void UpdateConfiguration(int maxPages)
        {
            _maxPagesToScrape = maxPages;
            _logger.LogInformation("Character Scraper configuration updated. Max pages: {maxPages}", maxPages);
        }
    }
}