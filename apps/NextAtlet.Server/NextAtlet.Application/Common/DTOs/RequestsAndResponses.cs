using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Common.DTOs;

// Identity (email + IdP subject) comes from the authenticated token, never the body.

/// <summary>Self-registration: the caller registers their own athlete profile.</summary>
public class RegisterOwnAthleteRequest
{
    public required string DisplayName { get; set; }
    public required string Slug { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public string DefaultLocaleId { get; set; } = default!;
    public string? GuardianEmail { get; set; } // Required if the caller is a minor
}

/// <summary>Guardian creates a profile for their child; the caller becomes the Guardian.</summary>
public class RegisterChildAthleteRequest
{
    public required string ChildDisplayName { get; set; }
    public required string Slug { get; set; }
    public required DateTime ChildDateOfBirth { get; set; }
    public string DefaultLocaleId { get; set; } = default!;
}

public class AthleteProfileDto
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public bool IsMinor { get; set; }
    public EnumerationDto DefaultLocale { get; set; } = default!;
}

public class SiteConfigDto
{
    public Guid Id { get; set; }
    public Guid AthleteProfileId { get; set; }
    public bool IsDraft { get; set; }

    // Typed value objects shared with the domain — no parallel DTO hierarchy, no dicts.
    public SiteLayout Layout { get; set; } = new();
    public GlobalSettings? GlobalSettings { get; set; }
    public int Version { get; set; }
}

public class UpdateSiteConfigRequest
{
    // System.Text.Json deserializes straight into the polymorphic section model
    // via the "type" discriminator — no JsonElement/Dictionary normalization needed.
    public SiteLayout Layout { get; set; } = new();
    public GlobalSettings? GlobalSettings { get; set; }
    public int ExpectedVersion { get; set; }
}

/// <summary>
/// Result of the /me domain-gate check. Registered = owns an athlete profile.
/// Role is the caller's ProfileRole id (athlete owner / guardian), or null if neither.
/// ProfileId = the owned profile (null if not an owner). GuardedProfileIds = profiles this caller
/// actively guards. PendingGuardianInvites = invitations awaiting this caller's acceptance.
/// </summary>
public record MeDto(
    bool Registered,
    string? Role,
    Guid? ProfileId,
    IReadOnlyList<Guid> GuardedProfileIds,
    int PendingGuardianInvites);

/// <summary>Body for inviting a person to an existing profile. Identity of the inviter comes from the token.</summary>
public class InviteToProfileRequest
{
    public required string Email { get; set; }
    public required string Role { get; set; } // ProfileRole id: "athlete_owner" | "guardian"
}

/// <summary>An issued invitation. The Id is the token used in the accept URL.</summary>
public record InvitationDto(Guid Id, Guid TargetProfileId, string Email, string Role, DateTime ExpiresUtc);

/// <summary>Result of accepting an invitation: which role on which profile was materialized.</summary>
public record InvitationAcceptedDto(Guid ProfileId, string Role);

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Details { get; set; }
}
