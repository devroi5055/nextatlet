using System.Security.Claims;
using NextAtlet.Application.Abstractions.Identity;

namespace NextAtlet.Api;

/// <summary>
/// Reads the authenticated caller from the validated token's claims on the current HTTP context.
/// Throws if there is no authenticated principal — JWT bearer authentication is wired in Step 2;
/// until then these endpoints require that integration to resolve a caller.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No active HTTP context.");

    public string AuthProviderId =>
        Principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal.FindFirstValue("sub")
        ?? throw new InvalidOperationException("No authenticated user (missing subject claim).");

    public string Email =>
        Principal.FindFirstValue(ClaimTypes.Email)
        ?? Principal.FindFirstValue("email")
        ?? throw new InvalidOperationException("Authenticated user has no email claim.");
}
