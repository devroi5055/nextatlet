using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Entities.ClubRegistry
{
    public class Club : AuditableEntity
    {
        // Identity from the source feed — never changes once set.
        public required string SourceKey { get; init; }
        public required string Source { get; init; }
        public required string CountryId { get; init; }

        // Re-imported from the source on every upsert.
        public required string Name { get; set; }
        public required string? Address { get; set; }
        public required DateTime LastImportedUtc { get; set; }

        /// <summary>
        /// Sports the club offers. Scraped sports are merged in on each import; sports an admin adds
        /// manually (not present in the source) are preserved across re-imports.
        /// </summary>
        public IReadOnlyList<string> SportIds { get; set; } = [];

        /// <summary>Soft-active flag. Set false by DeactivateMissing when the club drops out of the source feed.</summary>
        public bool IsActive { get; set; } = true;

        //navigation
        public IReadOnlyCollection<ClubOfficial> Officials { get; init; } = default!;

        public void AddSports(List<string> sports)
            => SportIds = SportIds.Union(sports).ToList();

        public void RemoveSports(List<string> sports)
            => SportIds = SportIds.Except(sports).ToList();
    }
}
