namespace scraper.Interfaces
{
    /// <summary>
    /// A simple set of methods to track the 3 states of a job. Can expand upon later.
    /// </summary>
    public interface IScrapeJobTracker
    {
        void StartJob(string jobId);
        void CompleteJob(string jobId);
        void FailJob(string jobId);
        string GetStatus(string jobId);
    }
}
