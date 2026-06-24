using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.ActionTokens.Strategies;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;

namespace NextAtlet.Application.Features.ActionTokens.Commands;

/// <summary>
/// The single accept endpoint for every emailed-link flow. The caller follows the link and
/// authenticates; this validates the token (exists, not expired, still pending) and then dispatches by
/// <see cref="ActionToken.Type"/> — the one place per-type completion logic lives. Identity comes from
/// the validated token claims, never the body. The token is single-use: <c>Accept</c> stamps it so a
/// link can't be replayed.
/// </summary>
public record AcceptActionTokenCommand(
    Guid TokenId,
    string? AuthProviderId) : IRequest<Result>;

public class AcceptActionTokenCommandHandler : IRequestHandler<AcceptActionTokenCommand, Result>
{
    private readonly IActionTokenRepository _tokens;
    private readonly ActionTokenStrategyRegistry _actionTokenStrategyRegistry;
    private readonly UserProvisioner _userProvisioner;
    private readonly IClock _clock;

    public AcceptActionTokenCommandHandler(
        IActionTokenRepository tokens,
        ActionTokenStrategyRegistry actionTokenStrategyRegistry,
        UserProvisioner userProvisioner,
        IClock clock)
    {
        _tokens = tokens;
        _actionTokenStrategyRegistry = actionTokenStrategyRegistry;
        _userProvisioner = userProvisioner;
        _clock = clock;
    }

    public async Task<Result> Handle(AcceptActionTokenCommand request, CancellationToken ct)
    {
        var token = await _tokens.GetByIdAsync(request.TokenId, ct);
        if (token is null)
            return Error.FromCode(ErrorCodes.ActionTokenNotFound);

        // Expiry is checked on use (no background sweeper needed for MVP).
        if (token.IsExpired)
            return Error.FromCode(ErrorCodes.ActionTokenExpired);

        if (!token.IsPending)
            return Error.FromCode(ErrorCodes.ActionTokenAlreadyUsed);


        var strategy = _actionTokenStrategyRegistry.Get(ActionTokenType.FromId(token.TypeId));

        var userResult = await AuthValidation(request.AuthProviderId, strategy, ct);
        if (userResult.IsFailure) return userResult.WithoutValue();

        Result strategyResult = await strategy.ExecuteAsync(token, userResult.Value, ct);
        if (strategyResult.IsFailure) return strategyResult;

        token.Accept(_clock.UtcNow);
        return Result.Success();
    }

    private async Task<Result<User?>> AuthValidation(string? authProviderId, IActionTokenStrategy strategy, CancellationToken ct)
    {
        if (strategy.authRequired == false)
            return Result<User?>.Success(null);

        if (authProviderId is null)
            return Error.FromCode(ErrorCodes.NotAuthorized);

        var actorUser = await _userProvisioner.TryGetAsync(authProviderId, ct);
        if (actorUser is null)
            throw new InvalidOperationException($"Authenticated subject '{authProviderId}' has no User row - provisioning invariant violated.");

        return Result<User?>.Success(actorUser);
    }
}
