namespace NextAtlet.Application.Contracts.Individuals.Request
{
    public class TransferControlRequest
    {
        public required string To { get; set; } // "athlete" | "guardian"
    }
}
