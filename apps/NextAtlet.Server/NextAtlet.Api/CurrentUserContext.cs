using System.Security.Claims;
using NextAtlet.Application.Abstractions.Identity;
using NextAtlet.Application.Common.Errors;

namespace NextAtlet.Api;

/// <summary>
/// Reads the authenticated caller from the validated JWT's claims on the current HTTP context.
/// Throws if there is no authenticated principal or required claims are missing.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _emailClaimType;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        // Auth0 access tokens don't include email by default — a custom (namespaced) claim is added
        // via an Action; point this at that claim type. Falls back to the standard email claims.
        _emailClaimType = configuration["Authentication:EmailClaimType"];
    }

    private ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No active HTTP context.");

    public string AuthProviderId =>
        Principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal.FindFirstValue("sub")
        ?? throw new DomainException(ErrorCodes.AuthSubMissing);

    // For Auth0, add a custom (namespaced) email claim via an Action and set Authentication:EmailClaimType.
    public string Email =>
        (_emailClaimType is not null ? Principal.FindFirstValue(_emailClaimType) : null)
        ?? Principal.FindFirstValue(ClaimTypes.Email)
        ?? Principal.FindFirstValue("email")
        ?? throw new DomainException(ErrorCodes.AuthEmailMissing);
}
