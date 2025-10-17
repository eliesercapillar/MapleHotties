namespace scraper.DTOs
{
    public class RankingsEntry
    {
        public string CharacterName { get; set; } = null!;
        public long Exp { get; set; }
        public int JobDetail { get; set; }
        public int JobID { get; set; }
        public int Level { get; set; }
        public int Rank { get; set; }
        public int WorldID { get; set; }
        public string CharacterImgURL { get; set; } = null!;
    }
}
