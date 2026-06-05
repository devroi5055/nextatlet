using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IProfileLoginRepository
{
    /// <summary>The caller's Active login on a profile (any role), or null. Feeds the PermissionResolver + the invite gate.</summary>
    Task<ProfileLogin?> GetActiveLoginAsync(Guid userId, Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>Ids of profiles this user actively guards (Active Guardian logins). Drives /me.</summary>
    Task<IReadOnlyList<Guid>> GetActiveGuardianProfileIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>True if the profile has an Active AthleteOwner login — transfer-to-athlete can't hand control to a ghost.</summary>
    Task<bool> HasActiveOwnerLoginAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>True if the profile has an Active Guardian login — transfer-to-guardian needs a guardian to receive it.</summary>
    Task<bool> HasActiveGuardianLoginAsync(Guid profileId, CancellationToken cancellationToken = default);

    void Add(ProfileLogin login);
}
