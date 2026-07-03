namespace NextAtlet.Application.Common.Errors;

/// <summary>
/// Single source of truth for user-facing error codes. Frontend catalog keys (da.json/en.json)
/// mirror these exactly — a build-time test enforces that every code has both translations.
/// Grow as features land.
/// </summary>
/// 
public static class ErrorCodes
{

    //Bad Ínput Values (400)
    public const string TransferTargetInvalid            = "control.transfer_target_invalid";
    public const string InvitationRoleInvalid            = "invitation.role_invalid";
    
    //Forbidden (403)
    public const string NotAuthorized                    = "not_authorized";
    public const string AuthSubMissing                   = "auth.sub_missing";
    public const string AuthEmailMissing                 = "auth.email_missing";

    //Not Found  (404)
    public const string DraftConfigNotFound              = "config.draft.not_found";
    public const string IndividualProfileNotFound        = "individual.profile_not_found";
    public const string ProfileNotFound                  = "profile.not_found";
    public const string OrganizationProfileNotFound      = "organization.profile_not_found";
    public const string ThemeNotFound                    = "theme.not_found";
    public const string SiteNotFound                     = "site.not_found";
    public const string ClubNotFound                     = "club.not_found";
    public const string UserNotFound                     = "user.not_found";
    public const string VerificationOfficialNotFound     = "verification.official_not_found";
    public const string ActionTokenNotFound              = "action_token.not_found";


    //Conflict (409)
    public const string SlugAlreadyTaken                 = "slug.already_taken";
    public const string SlugReserved                     = "slug.reserved";
    public const string DraftVersionConflict             = "config.draft.version_conflict";
    public const string SiteAlreadyExists                = "site.already_exists";
    public const string ConsentNotNeeded                 = "consent.not_needed";
    public const string NoAthleteLoginExists             = "control.no_athlete_login";
    public const string NoGuardianLoginExists            = "control.no_guardian_login";
    public const string InvitationAlreadyPending         = "invitation.already_pending";
    public const string ActionTokenAlreadyUsed           = "action_token.already_used";
    public const string ActionTokenExpired               = "action_token.expired";
    public const string VerificationOfficialEmailMissing = "verification.official_email_missing";


    //Unprocessable Entity (422) - Business Rule Violation
    public const string GuardianEmailRequired            = "guardian.email_required";
    public const string SectionTypeNotSupported          = "section.type_not_supported";
    public const string SectionNotPermitted              = "section.not_permitted";
    public const string SectionValidationFailed          = "section.validation_failed";
    public const string ThemeNotPermitted                = "theme.not_permitted";
    public const string GuardianCannotRegisterAdult      = "guardian.cannot_register_adult";
    public const string InvitationInvalidSiteRole        = "Invitation.invalid_site_role";
    public const string BelowMinimumAge                  = "registration.below_minimum_age";
    public const string ParentalConsentRequired          = "registration.parental_consent_required";
    public const string GuardianConsentRequired          = "consent.guardian_required";
    public const string AthleteTooYoungForControl        = "control.athlete_too_young";
    public const string InvitationEmailMismatch          = "invitation.email_mismatch";



    public const string Internal                = "internal_error";
}
