using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Sites;

public class ChangeRequest : AuditableEntity
{
    public required Guid TargetProfileId { get; set; }
    public required Guid ProposingOrganizationId { get; set; }
    public required Guid ProposedByUserId { get; set; }
    public required SiteLayout ProposedLayout { get; set; }
    public required Theme Theme { get; set; }
    public int ThemeVersion { get; set; } = 1;

    public string? PreviewImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
