namespace NextAtlet.Application.Contracts.Individuals.Request
{
    public class RegisterIndividualSiteGuardianRequest
    {
        public required string ChildDisplayName { get; set; }
        public required string Slug { get; set; }
        public required DateTime ChildDateOfBirth { get; set; }
        public string DefaultLocaleId { get; set; } = default!;
    }
}
