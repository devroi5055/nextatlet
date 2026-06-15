namespace NextAtlet.Application.Common.Options;

/// <summary>
/// Transactional-email settings, bound from the "Email" config section. When <see cref="ApiKey"/> is
/// empty the app falls back to the logging (no-send) transport — so local dev needs no secrets.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Resend";

    /// <summary>Resend API key. Empty = use the logging transport instead of sending real mail.</summary>
    public string InviteApiKey { get; set; } = string.Empty;

    /// <summary>Verified sender address (must belong to a domain verified in Resend).</summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>Display name shown alongside the sender address.</summary>
    public string FromName { get; set; } = "NextAtlet";

    /// <summary>
    /// Frontend base URL used to build the clickable accept link in invite emails:
    /// <c>{AppBaseUrl}/invitations/{id}/accept</c>. The frontend page handles sign-in, then calls the API.
    /// </summary>
    public string AppBaseUrl { get; set; } = string.Empty;
}
