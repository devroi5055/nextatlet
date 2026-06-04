namespace NextAtlet.Application.Abstractions.Identity;

/// <summary>
/// The authenticated caller, sourced from the validated OAuth token's claims (not the request body).
/// Implemented in the Api over the HTTP context; faked in tests. This is the trust boundary for
/// "who is acting" — handlers read identity from here, never from client-supplied fields.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>External IdP subject (the `sub` / NameIdentifier claim).</summary>
    string AuthProviderId { get; }

    /// <summary>The caller's email claim.</summary>
    string Email { get; }
}
