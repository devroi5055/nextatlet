using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Application.Contracts.Individuals.Response
{
    public class IndividualProfileResponse
    {
        public Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string DisplayName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public bool IsMinor { get; set; }
        public required ControlModes ControlMode { get; set; }
    }
}
