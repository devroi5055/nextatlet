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
    public const string DraftConfigNotFound     = "config.draft.not_found";
    public const string DraftVersionConflict    = "config.draft.version_conflict";
    public const string SectionTypeNotSupported = "section.type_not_supported";
    public const string SectionNotPermitted     = "section.not_permitted";
    public const string SectionValidationFailed = "section.validation_failed";
    public const string AthleteProfileNotFound  = "athlete.profile_not_found";
    public const string ProfileNotFound         = "profile.not_found";
    public const string OrganizationProfileNotFound = "organization.profile_not_found";
    public const string ThemeNotFound           = "theme.not_found";
    public const string ThemeNotPermitted       = "theme.not_permitted";
    public const string SiteNotFound         = "site.not_found";
    public const string SiteAlreadyExists    = "site.already_exists";
    public const string GuardianCannotRegisterAdult = "guardian.cannot_register_adult";

    // Age gating + consent (ControlMode plan).
    public const string BelowMinimumAge         = "registration.below_minimum_age";
    public const string ParentalConsentRequired = "registration.parental_consent_required";

    /// <summary>Publish blocked while a minor profile awaits guardian consent (GDPR Art. 8).</summary>
    public const string GuardianConsentRequired = "consent.guardian_required";
    public const string ConsentNotNeeded = "consent.not_needed";


    // Control transfer / collaboration.
    public const string AthleteTooYoungForControl = "control.athlete_too_young";
    public const string NoAthleteLoginExists    = "control.no_athlete_login";
    public const string NoGuardianLoginExists   = "control.no_guardian_login";
    public const string TransferTargetInvalid   = "control.transfer_target_invalid";

    // Invitations — the single, auditable home for pending profile invites.
    public const string InvitationNotFound      = "invitation.not_found";
    public const string InvitationAlreadyUsed   = "invitation.already_used";
    public const string InvitationExpired       = "invitation.expired";
    public const string InvitationEmailMismatch = "invitation.email_mismatch";
    public const string InvitationAlreadyPending = "invitation.already_pending";
    public const string InvitationRoleInvalid   = "invitation.role_invalid";

    /// <summary>Caller lacks the rights required for the requested action (e.g. inviting to a profile they don't hold).</summary>
    public const string NotAuthorized           = "not_authorized";

    // Authentication — the validated token is missing an expected claim.
    public const string AuthSubMissing          = "auth.sub_missing";
    public const string AuthEmailMissing        = "auth.email_missing";

    /// <summary>Generic fallback for unhandled/system failures. Never carries internal detail.</summary>
    public const string Internal                = "internal_error";
}
