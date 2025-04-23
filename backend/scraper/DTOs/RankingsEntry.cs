namespace scraper.DTOs
{
    public class RankingsEntry
    {
        public string? CharacterName { get; set; }
        public long Exp { get; set; }
        public int JobID { get; set; }
        public int Level { get; set; }
        public int Rank { get; set; }
        public string? WorldName { get; set; }
        public string? CharacterImgURL { get; set; }
    }
}
