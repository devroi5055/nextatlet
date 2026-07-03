namespace NextAtlet.Application.Common.DTOs
{
    public class ScrapedClub
    {
        public required string SourceKey { get; set; }
        public required string Source { get; set; }
        public required string Name { get; set; }
        public required string? Address { get; set; }
        public ICollection<string> Sports { get; set; } = default!;
        public ICollection<ScrapedClubOfficial> ScrapedOfficials { get; set; } = default!;
    }
}
