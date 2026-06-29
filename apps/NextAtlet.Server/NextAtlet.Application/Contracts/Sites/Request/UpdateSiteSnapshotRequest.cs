using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Contracts.Sites.Request
{
    public class UpdateSiteSnapshotRequest
    {
        // System.Text.Json deserializes straight into the polymorphic section model
        // via the "type" discriminator — no JsonElement/Dictionary normalization needed.
        public SiteLayout Layout { get; set; } = new();
        public GlobalSettings? GlobalSettings { get; set; }
        public int ExpectedVersion { get; set; }
    }
}
