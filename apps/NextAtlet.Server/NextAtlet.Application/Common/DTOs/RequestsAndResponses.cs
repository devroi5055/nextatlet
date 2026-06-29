using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Common.DTOs;

// Identity (email + IdP subject) comes from the authenticated token, never the body.




/// <summary>Self-registration: the caller registers their own athlete profile.</summary>


/// <summary>Guardian creates a profile for their child; the caller becomes the Guardian.</summary>
//public class RegisterIndividualSiteGuardianRequest
//{
//    public required string ChildDisplayName { get; set; }
//    public required string Slug { get; set; }
//    public required DateTime ChildDateOfBirth { get; set; }
//    public string DefaultLocaleId { get; set; } = default!;
//}

//public class SendOfficialEmailVerificationRequest
//{
//    public required Guid OrgSiteId { get; set; }
//    public required Guid ClubOfficialId { get; set; }

//}


//public class SiteDto
//{
//    public Guid Id { get; set; }
//    public required string Slug { get; set; }
//    public required string DisplayName { get; set; }
//    public required EnumerationDto DefaultLocale { get; set; } = default!;
//    public required EnumerationDto VisibilityState { get; set; }
//}

//public class IndividualProfileDto
//{
//    public Guid Id { get; set; }
//    public required string Slug { get; set; }
//    public required string DisplayName { get; set; }
//    public required DateOnly DateOfBirth { get; set; }
//    public bool IsMinor { get; set; }
//    public required ControlModes ControlMode { get; set; }
//}

//public class SiteSnapshotDto
//{
//    public Guid Id { get; set; }
//    public Guid SiteId { get; set; }

//    // Typed value objects shared with the domain — no parallel DTO hierarchy, no dicts.
//    public SiteLayout Layout { get; set; } = new();
//    public GlobalSettings? GlobalSettings { get; set; }
//    public int Version { get; set; }
//}

//public class UpdateAthleteSiteSnapshotRequest
//{
//    // System.Text.Json deserializes straight into the polymorphic section model
//    // via the "type" discriminator — no JsonElement/Dictionary normalization needed.
//    public SiteLayout Layout { get; set; } = new();
//    public GlobalSettings? GlobalSettings { get; set; }
//    public int ExpectedVersion { get; set; }
//}

/// <summary>
/// Result of the /me domain-gate check. Registered = owns an athlete profile.
/// Role is the caller's ProfileRole id (athlete owner / guardian), or null if neither.
/// ProfileId = the owned profile (null if not an owner). ControlMode / IsInControl / CanEdit describe
/// the caller's relationship to that owned profile (null/false for a guardian-only caller — the editor
/// loads each guarded child by id). GuardedProfileIds = profiles this caller actively guards.
/// PendingGuardianInvites = invitations awaiting this caller's acceptance.
/// The frontend uses IsInControl to show publish/transfer controls and CanEdit to show the draft editor.
/// </summary>
//public record MeDto(
//    bool Registered,
//    string? Role,
//    Guid? ProfileId,
//    ControlModes? ControlMode,
//    bool IsInControl,
//    bool CanEdit,
//    IReadOnlyList<Guid> GuardedProfileIds,
//    int PendingGuardianInvites);

/// <summary>Body for inviting a person to an existing profile. Identity of the inviter comes from the token.</summary>
//public class InviteToProfileRequest
//{
//    public required string Email { get; set; }
//    public required string Role { get; set; } // ProfileRole id: "athlete_owner" | "guardian"
//}

/// <summary>An issued invitation. The Id is the action-token used in the accept URL.</summary>
//public record InvitationDto(Guid Id, Guid TargetProfileId, string Email, string Role, DateTime ExpiresUtc);

/// <summary>
/// Result of accepting an action token. Type echoes which flow ran; TargetSiteId is the site acted on;
/// Role is the materialized login's role for an Invite, null for consent / org-verification.
/// </summary>
//public record ActionTokenAcceptedDto(string Type, Guid TargetSiteId, string? Role);

/// <summary>Body for transferring control of a profile to the other party.</summary>
//public class TransferControlRequest
//{
//    public required string To { get; set; } // "athlete" | "guardian"
//}

/// <summary>Body for toggling shared editing (collaboration) on a profile.</summary>
//public class SetCollaborationRequest
//{
//    public required bool SharedEditing { get; set; }
//}

//public class ErrorResponse
//{
//    public int StatusCode { get; set; }
//    public string Message { get; set; } = string.Empty;
//    public List<string>? Details { get; set; }
//}
