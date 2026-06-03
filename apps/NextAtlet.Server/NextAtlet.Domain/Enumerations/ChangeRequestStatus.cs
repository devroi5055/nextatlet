using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Enumerations;

public sealed class ChangeRequestStatus : Enumeration
{
    public static readonly ChangeRequestStatus Pending = new()
    {
        Id = "pending",
        Title = "Pending",
        Description = "Awaiting review by the athlete or guardian"
    };

    public static readonly ChangeRequestStatus Approved = new()
    {
        Id = "approved",
        Title = "Approved",
        Description = "Accepted — proposed sections have been merged into the draft"
    };

    public static readonly ChangeRequestStatus Rejected = new()
    {
        Id = "rejected",
        Title = "Rejected",
        Description = "Declined by the athlete or guardian. No changes were applied"
    };

    public static readonly ChangeRequestStatus Withdrawn = new()
    {
        Id = "withdrawn",
        Title = "Withdrawn",
        Description = "Retracted by the proposing organization before it was reviewed"
    };

    public static IReadOnlyCollection<ChangeRequestStatus> All =>
        [Pending, Approved, Rejected, Withdrawn];

    public static ChangeRequestStatus FromId(string id) =>
        All.FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown change request status: '{id}'");
}