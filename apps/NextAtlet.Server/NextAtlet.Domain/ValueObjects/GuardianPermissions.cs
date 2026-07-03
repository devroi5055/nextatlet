namespace NextAtlet.Domain.ValueObjects;


//TODO: Remove or change
/// <summary>
/// Guardian-only permissions config, stored as jsonb on ProfileLogin.
/// Null for AthleteOwner logins.
/// </summary>
public class LoginPermissions
{
    /// <summary>Whether the minor may edit their own content without guardian review.</summary>
    public bool MinorCanEditDraft { get; init; }

    /// <summary>Whether the minor may publish without guardian approval.</summary>
    public bool MinorCanPublish { get; init; }

    /// <summary>Whether the minor may approve incoming club change requests.</summary>
    public bool MinorCanApproveChanges { get; init; }

    /// <summary>Whether the minor may manage their own media assets.</summary>
    public bool MinorCanManageMedia { get; init; }

    /// <summary>Whether the minor may manage club memberships independently.</summary>
    public bool MinorCanManageMemberships { get; init; }

    /// <summary>
    /// Recommended defaults for a new guardian link.
    /// Minor may edit content but guardian retains publish and approval authority.
    /// </summary>
    public static LoginPermissions Defaults() => new()
    {
        MinorCanEditDraft = true,         // minor can edit freely
        MinorCanPublish = false,            // guardian must publish
        MinorCanApproveChanges = false,     // guardian approves club proposals
        MinorCanManageMedia = false,        // guardian manages media
        MinorCanManageMemberships = false   // guardian manages memberships
    };
}