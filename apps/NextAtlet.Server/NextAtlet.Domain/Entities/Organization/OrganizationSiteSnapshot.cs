using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Organization
{
    public class OrganizationSiteSnapshot : AuditableEntity
    {
        public required Guid OrganizationId { get; set; }
        public required Guid ThemeId { get; set; }
        public int ThemeVersion { get; set; } = 1;
        public required SiteLayout Layout { get; set; }
        public GlobalSettings? GlobalSettings { get; set; }
        public int Version { get; set; } = 1;
        public DateTime? PublishedUtc { get; set; }

        //Navigation
        public Theme Theme { get; set; } = default!;
        //public Organization Organization { get; set; } = default!;
    }
}
