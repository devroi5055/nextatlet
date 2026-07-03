using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Identity;

namespace NextAtlet.Domain.Entities.Identity;


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
    /// Null until the account is <see cref="IsClaimed">claimed</see> — e.g. a guardian who was
    /// invited by email but has not signed up yet. We never store password hashes; auth is delegated to the IdP.
    /// </summary>
    public string? AuthProviderId { get; set; }

    /// <summary>
    /// True once a real IdP identity is linked. An unclaimed user is a placeholder created by an
    /// invite (e.g. a guardian) that becomes claimed when that person first signs in.
    /// Computed from <see cref="AuthProviderId"/> — never stored.
    /// </summary>
    public bool IsClaimed => !string.IsNullOrWhiteSpace(AuthProviderId);

    // Navigation
    public ICollection<SiteLogin> SiteLogins { get; set; } = [];

    //TODO: implement OrganizationLogin and add navigation here
    //public ICollection<OrganizationLogin> OrganizationLogins { get; set; } = [];
}
