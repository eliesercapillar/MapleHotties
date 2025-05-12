using MapleTinder.Shared.Models.Entities;

namespace api.Interfaces
{
    public interface ICharacterScraperClient
    {
        Task<Character?> ScrapeCharacterAsync(string name);
        string TriggerScrapeAllAsync(int maxPages = 50000, int concurrency = 10);
        Task<string> GetScrapeStatus(string jobId);
    }
}