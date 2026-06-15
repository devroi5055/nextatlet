namespace NextAtlet.Application.Common.Options;

/// <summary>
/// The privacy-notice / terms version a guardian consents to (the "what-version" GDPR fact). A simple
/// configured string for MVP; formalise a versioning store only if terms change often. Stamped onto
/// each <c>GuardianConsent</c> so re-consent on a terms change is answerable later.
/// </summary>
public class TermsOptions
{
    public const string SectionName = "Terms";

    public string CurrentVersion { get; set; } = "2026-01";
}
