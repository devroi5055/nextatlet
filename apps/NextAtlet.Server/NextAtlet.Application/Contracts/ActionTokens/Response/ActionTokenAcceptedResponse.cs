namespace NextAtlet.Application.Contracts.ActionTokens.Response
{
    public class ActionTokenAcceptedResponse 
    {
        public required string Type { get; set; }
        public required Guid TargetSiteId { get; set; }
        public string? RoleId { get; set; }
    }
}
