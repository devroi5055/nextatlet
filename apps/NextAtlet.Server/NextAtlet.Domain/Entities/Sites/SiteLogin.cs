using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Organization;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Sites
{
    public class SiteLogin : AuditableEntity
    {
        public required Guid SiteId { get; set; }
        public required Guid UserId { get; set; }

        public required string SiteRoleId { get; set; }
        public required string StatusId { get; set; }

        public LoginPermissions? Permissions { get; set; }
        //navigation    
        public Site Site { get; set; } = default!;
        public User User { get; set; } = default!;

        public static SiteLogin CreateGuardian(Guid userId, Guid siteId) => new()
        {
            SiteId = siteId,
            UserId = userId,
            SiteRoleId = IndividualRole.Guardian.Id,
            StatusId = ProfileLoginStatus.Active.Id,
            Permissions = null
        };
        public static SiteLogin CreateAthlete(Guid userId, Guid siteId) => new()
        {
            SiteId = siteId,
            UserId = userId,
            SiteRoleId = IndividualRole.Owner.Id,
            StatusId = ProfileLoginStatus.Active.Id,
            Permissions = null
        };
    }
}
