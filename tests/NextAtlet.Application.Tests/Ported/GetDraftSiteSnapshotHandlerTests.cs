using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Sites;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Tests;

public class GetDraftSiteSnapshotHandlerTests
{
    private readonly ISiteSnapshotRepository _snapshots = Substitute.For<ISiteSnapshotRepository>();

    private GetDraftAthleteSiteSnapshotQueryHandler BuildHandler() =>
        new(_snapshots);

    // ── Sad path ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SnapshotNotFound_ThrowsDomainException()
    {
        _snapshots.GetCurrentDraftBySiteIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                  .Returns((SiteSnapshot?)null);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            BuildHandler().Handle(
                new GetDraftAthleteSiteSnapshotQuery(Guid.NewGuid()),
                CancellationToken.None));

        Assert.Equal(ErrorCodes.DraftConfigNotFound, ex.ErrorCode);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SnapshotFound_ReturnsMappedDto()
    {
        var siteId   = Guid.NewGuid();
        var themeId  = Guid.NewGuid();
        var snapshot = new SiteSnapshot
        {
            SiteId  = siteId,
            ThemeId = themeId,
            Layout  = new SiteLayout { Sections = [] },
            GlobalSettings = new GlobalSettings { AccentColor = "#fff", FontFamily = "Inter" }
        };
        _snapshots.GetCurrentDraftBySiteIdAsync(siteId, Arg.Any<CancellationToken>())
                  .Returns(snapshot);

        var result = await BuildHandler().Handle(
            new GetDraftAthleteSiteSnapshotQuery(siteId), CancellationToken.None);

        Assert.Equal(snapshot.Id,     result.Id);
        Assert.Equal(siteId,          result.SiteId);
        Assert.NotNull(result.Layout);
        Assert.NotNull(result.GlobalSettings);
    }
}
