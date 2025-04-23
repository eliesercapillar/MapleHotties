namespace scraper.DTOs
{
    public class RankingsApiResponse
    {
        public int TotalCount { get; set; }
        public List<RankingsEntry>? Ranks { get; set; }
    }
}
