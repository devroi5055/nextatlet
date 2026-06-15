using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Domain.Common
{
    public class OrgVerification
    {
        public Guid? VerifiedByUserId { get; set; }
        public string? MethodId { get; set; }
        public int? CVR { get; set; }
        public DateTime VerifiedUtc {  get; set; }
    }
}
