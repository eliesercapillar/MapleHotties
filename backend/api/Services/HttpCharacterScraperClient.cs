using api.Interfaces;
using MapleTinder.Shared.Models.Entities;

namespace api.Services
{
    public class HttpCharacterScraperClient : ICharacterScraperClient
    {
        private readonly HttpClient _http;

        public HttpCharacterScraperClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<Character> ScrapeCharacterAsync(string name)
        {
            var response = await _http.PostAsync($"/scrape/character/{name}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Character>();
        }

        public async Task TriggerScrapeAllAsync(int maxPages = 50000)
        {
            var response = await _http.PostAsync($"/scrape/all?maxPages={maxPages}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
