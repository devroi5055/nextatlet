namespace NextAtlet.Domain.Authorization;

/// <summary>
/// The effective capability of a login on a profile — always derived at request time from
/// <c>ControlMode</c> + role by the <see cref="PermissionResolver"/>, never stored per login. Three
/// presets: <see cref="FullControl"/> (the controller), <see cref="EditOnly"/> (the other party in a
/// Shared mode), <see cref="ReadOnly"/> (the other party otherwise).
/// </summary>
public record ProfilePermissions(
    bool CanEditContent,
    bool CanPublish,
    bool CanApproveChanges,
    bool CanManageMedia,
    bool CanManageMemberships)
{
    /// <summary>No/revoked login. Flag-wise identical to <see cref="ReadOnly"/> for now — kept distinct so it can diverge later.</summary>
    public static readonly ProfilePermissions None = new(false, false, false, false, false);

    /// <summary>Login exists but is not the controller (non-shared mode). Observe only.</summary>
    public static readonly ProfilePermissions ReadOnly = new(false, false, false, false, false);

    /// <summary>The other party in a Shared mode: collaborate on the draft (+ media), but the senior acts stay with the controller.</summary>
    public static readonly ProfilePermissions EditOnly = new(
        CanEditContent: true, CanPublish: false, CanApproveChanges: false,
        CanManageMedia: true, CanManageMemberships: false);

    /// <summary>The controlling party: every act, including the senior ones (publish, approve, transfer).</summary>
    public static readonly ProfilePermissions FullControl = new(true, true, true, true, true);
}
