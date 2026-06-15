using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Billing;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Domain.Entities.Organization
{
    public class Organization : AuditableEntity
    {
        public required string OrganizationTypeId { get; set; }
        public required string Slug { get; set; }
        public required string DisplayName { get; set; }
        public bool IsServerManaged { get; set; } = false;
        public string OrganizationTierId { get; set; } = OrganizationTier.Free.Id;
        public int? AthleteSlotCount { get; set; } 
        public string VisibilityStateId { get; set; } = VisibilityState.Public.Id;
        public string VerificationStatusId { get; set; } = VerificationStatus.Pending.Id;
        public OrgVerification? Verification { get; set; }

        //navigations
        public ICollection<OrganizationLogin> Logins { get; set; } = [];
    }
}
