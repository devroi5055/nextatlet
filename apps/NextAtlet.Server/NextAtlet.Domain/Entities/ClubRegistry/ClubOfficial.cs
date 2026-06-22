using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Domain.Entities.Verification
{
    public class ClubOfficial : AuditableEntity
    {
        public required Guid ClubId{ get; set; }
        public required string Name { get; set; }
        public required string? Email { get; set; }
        public required string? Phone { get; set; }
        public required string RoleId { get; set; } = ClubOfficialRole.Other.Id;
    }
}
