using scraper.Interfaces;
using System.Collections.Concurrent;

namespace scraper.Services
{
    public class ScrapeJobTracker : IScrapeJobTracker
    {
        private readonly ConcurrentDictionary<string, string> _jobStatuses = new();

        public void StartJob(string jobId) => _jobStatuses[jobId] = "Running";
        public void CompleteJob(string jobId) => _jobStatuses[jobId] = "Completed";
        public void FailJob(string jobId) => _jobStatuses[jobId] = "Failed";
        public string GetStatus(string jobId) => _jobStatuses.TryGetValue(jobId, out var status) ? status : null!;
    }
}
