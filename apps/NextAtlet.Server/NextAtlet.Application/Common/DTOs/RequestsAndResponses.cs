using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Common.DTOs;

public class CreateAthleteRequest
{
    // Owner identity (email + IdP subject) comes from the authenticated token, not the body.
    public required string DisplayName { get; set; }
    public required string Slug { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public EnumerationDto DefaultLocale { get; set; } = default!;
    public string? GuardianEmail { get; set; } // Required if athlete is a minor
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
/// </summary>
public record MeDto(bool Registered, string? Role);

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Details { get; set; }
}
