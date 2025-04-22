using api.Interfaces;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Mvc;

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

        public string TriggerScrapeAllAsync(int maxPages = 50000)
        {
            var jobId = Guid.NewGuid().ToString();

            _ = Task.Run(() => _http.PostAsync($"/scrape/all?maxPages={maxPages}&jobId={jobId}", null));

            return jobId;
        }

        public async Task<string> GetScrapeStatus(string jobId)
        {
            var response = await _http.GetAsync($"/scrape/status/{jobId}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
