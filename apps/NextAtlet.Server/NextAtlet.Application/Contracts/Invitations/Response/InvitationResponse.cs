namespace NextAtlet.Application.Contracts.Invitations.Response;

/// <summary>An issued invitation. The Id is the action-token used in the accept URL.</summary>
public record InvitationResponse(Guid Id, Guid TargetProfileId, string Email, string Role, DateTime ExpiresUtc);
