using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Contracts.Sites.Response
{
    public class SiteSnapshotResponse
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }

        // Typed value objects shared with the domain — no parallel DTO hierarchy, no dicts.
        public SiteLayout Layout { get; set; } = new();
        public GlobalSettings? GlobalSettings { get; set; }
        public int Version { get; set; }
    }
}
