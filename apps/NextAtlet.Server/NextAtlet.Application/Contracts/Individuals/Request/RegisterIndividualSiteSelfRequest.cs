namespace NextAtlet.Application.Contracts.Individuals.Request
{
    public class RegisterIndividualSiteSelfRequest
    {
        public required string DisplayName { get; set; }
        public required string Slug { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public string DefaultLocaleId { get; set; } = default!;
        public string? GuardianEmail { get; set; }        // Required for 13–17
    }
}
