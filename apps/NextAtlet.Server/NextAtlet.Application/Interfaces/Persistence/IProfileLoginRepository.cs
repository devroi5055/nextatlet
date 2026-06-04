using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IProfileLoginRepository
{
    /// <summary>True if the user holds any Guardian login (any status).</summary>
    Task<bool> HasGuardianLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>The user's tracked Pending guardian logins (the invites awaiting acceptance).</summary>
    Task<IReadOnlyList<ProfileLogin>> GetPendingGuardianLoginsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(ProfileLogin login);
}
