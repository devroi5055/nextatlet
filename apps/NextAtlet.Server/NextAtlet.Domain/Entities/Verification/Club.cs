using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Domain.Entities.Verification
{
    public class Club : AuditableEntity
    {
        public required string SourceKey { get; init; }
        public required string Source { get; init; }
        public required string Name { get; init; }
        public  required string CountryId { get; init; }
        public IReadOnlyList<string> SportIds { get; init; } = [];
        public required string Address { get; init; }
        public required DateTime LastImportedUtc { get; init; }

        //navigation
        public IReadOnlyCollection<ClubOfficial> Officials { get; init; } = default!;
    }
}
