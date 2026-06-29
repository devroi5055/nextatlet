using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Contracts.Identity.Response
{
    public record MeResponse
    {
        public required bool Registered { get; init; }
        public string? Role { get; init; }
        public Guid? ProfileId { get; init; }
        public ControlModes? ControlMode { get; init; }
        public bool IsInControl { get; init; }
        public bool CanEdit { get; init; }
        public required IReadOnlyList<Guid> GuardedProfileIds { get; init; }
        public int PendingGuardianInvites { get; init; }
    }
}
