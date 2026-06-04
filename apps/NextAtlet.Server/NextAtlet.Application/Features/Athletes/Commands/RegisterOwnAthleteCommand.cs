using MediatR;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Features.Athletes.Commands;

/// <summary>
/// Self-registration: the authenticated caller registers an AthleteProfile for themselves and
/// becomes its AthleteOwner. If the caller is a minor, a guardian must be invited (created in the
/// same transaction) — the core "minor profile always has a guardian" rule.
/// Identity (sub/email) comes from the authenticated principal via the controller, never the body.
/// </summary>
public record RegisterOwnAthleteCommand(
    string AuthProviderId,
    string Email,
    string DisplayName,
    string Slug,
    DateTime DateOfBirth,
    string DefaultLocaleId,
    string? GuardianEmail = null) : IRequest<AthleteProfileDto>;

public class RegisterOwnAthleteCommandHandler
    : AthleteRegistrationHandlerBase, IRequestHandler<RegisterOwnAthleteCommand, AthleteProfileDto>
{
    public RegisterOwnAthleteCommandHandler(
        IUserRepository users,
        IAthleteProfileRepository profiles,
        IProfileLoginRepository logins,
        IThemeRepository themes,
        ISiteConfigRepository siteConfigs,
        IUnitOfWork unitOfWork)
        : base(users, profiles, logins, themes, siteConfigs, unitOfWork) { }

    public async Task<AthleteProfileDto> Handle(RegisterOwnAthleteCommand request, CancellationToken cancellationToken)
    {
        var caller = await GetOrCreateUserAsync(request.Email, request.AuthProviderId, cancellationToken);

        // A user can't self-register two owned profiles.
        if (await Profiles.GetOwnedByUserIdAsync(caller.Id, cancellationToken) is not null)
            throw new DomainException(ErrorCodes.ProfileAlreadyExists);

        var isMinor = IsMinor(request.DateOfBirth);
        if (isMinor && string.IsNullOrWhiteSpace(request.GuardianEmail))
            throw new DomainException(ErrorCodes.GuardianEmailRequired);

        var profile = await CreateAthleteProfileCoreAsync(request.Slug, request.DisplayName, request.DateOfBirth, request.DefaultLocaleId, cancellationToken);

        Logins.Add(ProfileLogin.CreateOwner(caller.Id, profile.Id));

        // Minor → invite a (pending) guardian in the SAME transaction; the invariant is never violated.
        if (isMinor)
        {
            var guardian = await GetOrCreatePendingUserAsync(request.GuardianEmail!, cancellationToken);
            Logins.Add(ProfileLogin.CreateGuardian(guardian.Id, profile));
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(profile);
    }
}
