using NextAtlet.Domain.Common;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Enumerations.Billing;

/// <summary>Internal catalog filter.</summary>
public sealed class PlanAudience : Enumeration
{
    public static readonly PlanAudience Athlete = new()
    {
        Id = "athlete",
        Title = new LocalizedText { Da = "Atlet", En = "Athlete" },
        Description = new LocalizedText { Da = "Plan beregnet til individuelle atleter", En = "Plan intended for individual athletes" }
    };

    public static readonly PlanAudience Organization = new()
    {
        Id = "organization",
        Title = new LocalizedText { Da = "Organisation", En = "Organization" },
        Description = new LocalizedText { Da = "Plan beregnet til klubber og organisationer", En = "Plan intended for clubs and organizations" }
    };

    public static IReadOnlyCollection<PlanAudience> All => [Athlete, Organization];

    public static PlanAudience FromId(string id) =>
        All.FirstOrDefault(a => a.Id == id)
        ?? throw new ArgumentException($"Unknown plan audience: '{id}'");
}
