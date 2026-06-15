using System.Security.Claims;
using NextAtlet.Application.Common.Errors;

namespace NextAtlet.Api;

/// <summary>
/// Reads identity claims from the authenticated principal, scheme-agnostically — the subject and
/// email may surface under the raw OIDC name ("sub"/"email") or the mapped .NET claim types
/// depending on which scheme authenticated the request (bearer vs cookie). Handlers never read the
/// request body for identity; controllers use these to populate command params.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Optional custom email claim type, set once at startup from <c>Authentication:EmailClaimType</c>.
    /// Auth0 access tokens omit <c>email</c> by default — an Action adds it as a namespaced claim
    /// (e.g. <c>https://nextatlet.dk/email</c>); point this at that claim type to read it.
    /// </summary>
    public static string? ConfiguredEmailClaimType { get; set; }

    public static string GetAuthProviderId(this ClaimsPrincipal user) =>
        user.FindFirst("sub")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new DomainException(ErrorCodes.AuthSubMissing);

    public static string GetEmail(this ClaimsPrincipal user)
    {
        if (!string.IsNullOrWhiteSpace(ConfiguredEmailClaimType)
            && user.FindFirst(ConfiguredEmailClaimType)?.Value is { Length: > 0 } custom)
        {
            return custom;
        }

        return user.FindFirst("https://nextatlet.com/email")?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new DomainException(ErrorCodes.AuthEmailMissing);
    }
}
