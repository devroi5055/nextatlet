using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Membership;

namespace NextAtlet.Domain.Entities.Sites
{
    public class Membership : AuditableEntity
    {
        public required Guid IndividualProfileId { get; set; }
        public required Guid OrganizationId { get; set; }
        public required string RoleId { get; set; }
        public DateTime? EndDate { get; set; }
        public string statusId { get; set; } = MembershipStatus.Active.Id;
        public bool OccupiesSlot { get; set; } = true;
    }
}
