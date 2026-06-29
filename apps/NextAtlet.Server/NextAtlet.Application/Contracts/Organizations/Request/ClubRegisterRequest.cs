namespace NextAtlet.Application.Contracts.Organizations.Request
{
    public class ClubRegisterRequest
    {
        public required string DisplayName { get; set; }
        public required string Slug { get; set; }
        public required string PlanTierId { get; set; }
        public string DefaultLocaleId { get; set; } = default!;
    }
}
