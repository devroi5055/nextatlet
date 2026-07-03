namespace NextAtlet.Application.Abstractions.Persistence;

/// <summary>
/// Commits all pending repository changes in a single transaction.
/// Owned by the handler (orchestrator), not the repositories — commit timing stays explicit.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
