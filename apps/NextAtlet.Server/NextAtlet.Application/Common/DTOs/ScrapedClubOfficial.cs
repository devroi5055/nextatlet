namespace NextAtlet.Application.Common.DTOs
{
    public class ScrapedClubOfficial
    {
        public required string Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public required string Role { get; set; }
    }
}
