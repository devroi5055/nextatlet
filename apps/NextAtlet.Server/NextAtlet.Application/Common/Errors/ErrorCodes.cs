namespace NextAtlet.Application.Common.Errors;

/// <summary>
/// Single source of truth for user-facing error codes. Frontend catalog keys (da.json/en.json)
/// mirror these exactly — a build-time test enforces that every code has both translations.
/// Grow as features land.
/// </summary>
public static class ErrorCodes
{
    public const string SlugAlreadyTaken        = "slug.already_taken";
    public const string SlugReserved            = "slug.reserved";
    public const string GuardianEmailRequired   = "guardian.email_required";
    public const string GuardianInviteNotFound  = "guardian.invitation.not_found";
    public const string ProfileNotFound         = "profile.not_found";
    public const string DraftConfigNotFound     = "config.draft.not_found";
    public const string DraftVersionConflict    = "config.draft.version_conflict";
    public const string SectionTypeNotSupported = "section.type_not_supported";
    public const string SectionValidationFailed = "section.validation_failed";

    /// <summary>Generic fallback for unhandled/system failures. Never carries internal detail.</summary>
    public const string Internal                = "internal_error";
}
