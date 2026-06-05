using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IProfileLoginRepository
{
    /// <summary>True if the user holds any Active login (any role) on the given profile — the invite gate.</summary>
    Task<bool> HasActiveLoginAsync(Guid userId, Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>Ids of profiles this user actively guards (Active Guardian logins). Drives /me.</summary>
    Task<IReadOnlyList<Guid>> GetActiveGuardianProfileIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(ProfileLogin login);
}
