namespace NextAtlet.Domain.Entities;

public class ProfileLogin
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid AthleteProfileId { get; set; }
    public required string Role { get; set; } // "AthleteOwner", "Guardian"
    public Dictionary<string, object>? Permissions { get; set; } // jsonb
    public required string Status { get; set; } = "Active"; // "Pending", "Active", "Revoked"
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public AthleteProfile? AthleteProfile { get; set; }
}
