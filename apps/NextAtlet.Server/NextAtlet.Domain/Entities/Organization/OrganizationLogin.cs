using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Domain.Entities.Organization
{
    public class OrganizationLogin : AuditableEntity
    {
        public required Guid OrganizationId { get; set; }
        public required Guid UserId { get; set; }

        public required string OrganizationRoleId { get; set; }
        public required string StatusId { get; set; } = OrganizationLoginStatus.Active.Id;

        //navigation
        public Organization Organization { get; set; } = default!;
    }
}
