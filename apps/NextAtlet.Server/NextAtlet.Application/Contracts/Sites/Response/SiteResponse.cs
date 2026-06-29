using NextAtlet.Application.Common.DTOs;

namespace NextAtlet.Application.Contracts.Sites.Response
{
    public class SiteResponse
    {
        public Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string DisplayName { get; set; }
        public required EnumerationDto DefaultLocale { get; set; } = default!;
        public required EnumerationDto VisibilityState { get; set; }
    }
}
