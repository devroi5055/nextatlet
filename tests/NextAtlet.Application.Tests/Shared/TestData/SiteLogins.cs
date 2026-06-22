using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="SiteLogin"/> via the entity's own factories (which encode the
/// role + Active status + no stored permissions). Revoked variants flip the status afterwards.
/// </summary>
public static class ProfileLogins
{
    public static SiteLogin AnOwnerLogin(Guid? userId = null, Guid? profileId = null)
        => SiteLogin.CreateAthlete(userId ?? Guid.NewGuid(), profileId ?? Guid.NewGuid());

    public static SiteLogin AGuardianLogin(Guid? userId = null, Guid? profileId = null)
        => SiteLogin.CreateGuardian(userId ?? Guid.NewGuid(), profileId ?? Guid.NewGuid());

    public static SiteLogin ARevokedOwnerLogin(Guid? userId = null, Guid? profileId = null)
        => Revoke(AnOwnerLogin(userId, profileId));

    public static SiteLogin ARevokedGuardianLogin(Guid? userId = null, Guid? profileId = null)
        => Revoke(AGuardianLogin(userId, profileId));

    private static SiteLogin Revoke(SiteLogin login)
    {
        login.StatusId = ProfileLoginStatus.Revoked.Id;
        return login;
    }
}
