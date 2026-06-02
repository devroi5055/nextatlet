namespace NextAtlet.Application.DTOs;

public class CreateAthleteRequest
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string Slug { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public string DefaultLocale { get; set; } = "da";
    public string? GuardianEmail { get; set; } // Required if athlete is a minor
}

public class AthleteProfileDto
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public bool IsMinor { get; set; }
    public string DefaultLocale { get; set; } = "da";
}

public class SiteConfigDto
{
    public Guid Id { get; set; }
    public Guid AthleteProfileId { get; set; }
    public string State { get; set; } = "Draft";
    public Dictionary<string, object> Layout { get; set; } = new();
    public Dictionary<string, object>? GlobalSettings { get; set; }
    public int Version { get; set; }
}

public class UpdateSiteConfigRequest
{
    public Dictionary<string, object> Layout { get; set; } = new();
    public Dictionary<string, object>? GlobalSettings { get; set; }
    public int ExpectedVersion { get; set; }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Details { get; set; }
}
