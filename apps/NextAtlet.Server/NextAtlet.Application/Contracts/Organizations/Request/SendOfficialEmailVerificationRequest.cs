namespace NextAtlet.Application.Contracts.Organizations.Request
{
    public class SendOfficialEmailVerificationRequest
    {
        public required Guid OrgSiteId { get; set; }
        public required Guid ClubOfficialId { get; set; }

    }
}
