using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="ProfileLogin"/> via the entity's own factories (which encode the
/// role + Active status + no stored permissions). Revoked variants flip the status afterwards.
/// </summary>
public static class ProfileLogins
{
    public static ProfileLogin AnOwnerLogin(Guid? userId = null, Guid? profileId = null)
        => ProfileLogin.CreateOwner(userId ?? Guid.NewGuid(), profileId ?? Guid.NewGuid());

    public static ProfileLogin AGuardianLogin(Guid? userId = null, Guid? profileId = null)
        => ProfileLogin.CreateGuardian(userId ?? Guid.NewGuid(), profileId ?? Guid.NewGuid());

    public static ProfileLogin ARevokedOwnerLogin(Guid? userId = null, Guid? profileId = null)
        => Revoke(AnOwnerLogin(userId, profileId));

    public static ProfileLogin ARevokedGuardianLogin(Guid? userId = null, Guid? profileId = null)
        => Revoke(AGuardianLogin(userId, profileId));

    private static ProfileLogin Revoke(ProfileLogin login)
    {
        login.Status = ProfileLoginStatus.Revoked;
        return login;
    }
}
