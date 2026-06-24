using System.Text.Json.Serialization;

namespace NextAtlet.Domain.Entities.Identity;

/// <summary>
/// Per-purpose data carried by an <see cref="ActionToken"/>, persisted as jsonb. Polymorphism is
/// driven by the "type" discriminator — the same typed-payload pattern as SectionData, never a loose
/// dictionary. To add a token kind: add an <see cref="Enumerations.Identity.ActionTokenType"/> value,
/// a payload subclass here with a [JsonDerivedType] entry, and an accept case in AcceptActionTokenCommand.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(InvitePayload), "invite")]
[JsonDerivedType(typeof(ConsentPayload), "consent")]
[JsonDerivedType(typeof(OrgEmailVerificationPayload), "orgEmailVerification")]
public abstract class ActionTokenPayload { }

/// <summary>Invite a person (by email) to a site in a given role. The email is the accept-time match gate.</summary>
public sealed class InvitePayload : ActionTokenPayload
{
    public required string Email { get; set; }

    /// <summary>The <see cref="Enumerations.Individual.IndividualRole"/> id to grant on accept.</summary>
    public required string RoleId { get; set; }
}

/// <summary>Guardian-consent request for a minor's site. Records the audited terms version on accept.</summary>
public sealed class ConsentPayload : ActionTokenPayload
{
    public required string Email { get; set; }

    /// <summary>The privacy-notice version the guardian is consenting to (WHAT, for the audit row).</summary>
    public required string TermsVersion { get; set; }
}

/// <summary>Email-to-official organization verification. The official is sourced from the trusted ClubRegistry.</summary>
public sealed class OrgEmailVerificationPayload : ActionTokenPayload
{
    /// <summary>Which registry official the link was sent to — the authority basis + audit fact.</summary>
    public required Guid ClubOfficialId { get; set; }
    public required Guid? UserId { get; set; }
    public required string Email { get; set; }
}
