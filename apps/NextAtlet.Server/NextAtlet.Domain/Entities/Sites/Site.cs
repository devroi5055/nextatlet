using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Organization;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Domain.Entities.Sites
{
    public class Site : AuditableEntity
    {
        public Guid? CurrentDraftSnapshotId { get; set; }
        public Guid? CurrentPublishedSnapshotId { get; set; }
        public required string Slug { get; set; }
        public required string DisplayName { get; set; }
        public string VisibilityStateId { get; set; } = VisibilityStates.Public.Id;
        public string VerificationStatusId { get; set; } = VerificationStatus.Pending.Id;
        public string DefaultLocaleId { get; set; } = Locale.En.Id;
        public required string SiteTypeId { get; set; } = SiteType.Individual.Id;

        // Navigation
        public SiteSnapshot? CurrentDraftSnapshot { get; set; }
        public SiteSnapshot? CurrentPublishedSnapshot { get; set; }
        public ICollection<SiteLogin> SiteLogins { get; set; } = [];
        public ICollection<MediaAsset> MediaAssets { get; set; } = [];
    }
}
