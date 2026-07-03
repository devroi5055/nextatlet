namespace NextAtlet.Application.Contracts.Invitations.Request
{
    public class InviteToSiteRequest
    {
        public required string Email { get; set; }
        public required string Role { get; set; } // ProfileRole id: "athlete_owner" | "guardian"
    }
}
