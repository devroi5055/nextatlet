using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IProfileLoginRepository
{
    /// <summary>True if the user holds any Guardian login.</summary>
    Task<bool> HasGuardianLoginAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(ProfileLogin login);
}
