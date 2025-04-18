using MapleTinder.Shared.Models.Entities;

namespace api.Interfaces
{
    public interface ICharacterScraperClient
    {
        Task<Character> ScrapeCharacterAsync(string name);
        Task TriggerScrapeAllAsync(int maxPages = 50000);
    }
}