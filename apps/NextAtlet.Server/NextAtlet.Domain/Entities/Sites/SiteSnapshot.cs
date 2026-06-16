using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Entities.Sites
{

    public class SiteSnapshot : CreatedOnlyEntity
    {
        public required Guid SiteId { get; set; }
        public required Guid ThemeId { get; set; }
        public required SiteLayout Layout { get; set; }
        public GlobalSettings? GlobalSettings { get; set; }
        public DateTime? PublishedUtc { get; set; }

        //Navigation
        public Theme Theme { get; set; } = default!;
        //public Site Site { get; set; } = default!;
    }
}
