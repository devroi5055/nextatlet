using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Tests.Shared.TestData;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NextAtlet.Application.Tests.Athletes.Commands;

public class EditDraftAthleteSiteSnapshotCommandTests
{
    [Fact]
    public async Task Fails_WhenAthleteProfile_NotFound()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteSiteId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteSiteId, layout, null, 2);

        // Business rejection — the caller addressed a site that isn't there.
        var result = await fixture.Handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.SiteNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task Throws_WhenDraft_NotFound()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        var profile = TestAthletes.AGuardianControlledProfile();
        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(profile);
        fixture.SiteSnapshotRepository.GetDraftByProfileIdAsync(Arg.Any<Guid>()).ReturnsNull();

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteProfileId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteProfileId, layout, null, 2);

        // Broken invariant — an existing site must have a draft. Surfaces as an exception (→ 500), not a Result.
        await Assert.ThrowsAsync<InvalidOperationException>(
          () => fixture.Handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Fails_WhenVersion_Conflict()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        var profile = TestAthletes.AGuardianControlledProfile();
        var snapshot = AthleteSiteSnapshots.ADraftSiteSnapshot();

        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(profile);
        fixture.SiteSnapshotRepository.GetDraftByProfileIdAsync(Arg.Any<Guid>()).Returns(snapshot);

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteProfileId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteProfileId, layout, null, 2);

        // Optimistic concurrency is a user-recoverable rejection (reload + retry) → Result, not an exception.
        var result = await fixture.Handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.DraftVersionConflict, result.Error!.Code);
    }

    [Fact]
    public async Task Throws_WhenSnapshotTheme_NotFound()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        var profile = TestAthletes.AGuardianControlledProfile();

        var snapshot = AthleteSiteSnapshots.ADraftSiteSnapshot();
        var theme = AthleteSiteSnapshots.DefaultFreeTheme();

        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(profile);
        fixture.SiteSnapshotRepository.GetDraftByProfileIdAsync(Arg.Any<Guid>()).Returns(snapshot);
        fixture.ThemeRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteProfileId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteProfileId, layout, null, 1);

        // The draft references a theme that must resolve — a broken invariant, surfaced as an exception.
        await Assert.ThrowsAsync<InvalidOperationException>(
          () => fixture.Handler.Handle(command, CancellationToken.None));
    }
}
