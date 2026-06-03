

using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Domain.Entities.Shared;


/// <summary>
/// The login credential. One real person may hold one login used across roles,
/// or distinct logins. Has no inherent allegiance to profiles or organizations —
/// that is established by ProfileLogin and OrganizationLogin respectively.
/// </summary>
public class User : AuditableEntity
{
    public required string Email { get; set; }

    /// <summary>
    /// External IdP subject identifier (e.g. Auth0/Entra sub claim).
    /// We do not store password hashes — auth is delegated to the IdP.
    /// </summary>
    public required string AuthProviderId { get; set; }

    // Navigation
    public ICollection<ProfileLogin> ProfileLogins { get; set; } = [];

    //TODO: implement OrganizationLogin and add navigation here
    //public ICollection<OrganizationLogin> OrganizationLogins { get; set; } = [];
}
