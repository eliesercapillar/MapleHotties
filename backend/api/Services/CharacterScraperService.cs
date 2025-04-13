using api.Data;

namespace api.Services
{
    /// <summary>
    /// Scrapes the Official Maplestory Rankings site for the latest  
    /// </summary>
    public class CharacterScraperService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CharacterScraperService> _logger;

        public CharacterScraperService(IServiceProvider services, ILogger<CharacterScraperService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MapleTinderDbContext>();

                    // TODO: Put your scraping logic here
                    // Use HttpClient + HtmlAgilityPack to scrape the MapleStory ranking site
                    // Then use dbContext.MapleCharacters.Add/Update to persist

                    _logger.LogInformation("Character scraping done at: {time}", DateTimeOffset.Now);
                }

                // Run every 6 hours (adjust as needed)
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
