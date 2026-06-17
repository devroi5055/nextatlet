using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Billing;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Domain.Entities.Sites
{
    public class OrganizationProfile : AuditableEntity
    {
        public required Guid SiteId { get; set; }
        public bool IsServerManaged { get; set; } = false;
        public int AthleteSlotCount { get; set; } = 10;
        public string OrganizationTierId { get; set; } = OrganizationTier.Free.Id;
        public string VerificationStatusId { get; set; } = VerificationStatus.Pending.Id;
        public required string OrganizationTypeId { get; set; } = OrganizationType.Club.Id;
        public OrgVerification? Verification { get; set; }

        //navigations
        //public Site Site { get; set; } = default!;
    }
}
