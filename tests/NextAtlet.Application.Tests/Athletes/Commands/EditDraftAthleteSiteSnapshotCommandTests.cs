using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NextAtlet.Application.Tests.Athletes.Commands;

public class EditDraftAthleteSiteSnapshotCommandTests
{
    [Fact]
    public async Task ThrowsError_WhenAthleteProfile_NotFound()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteSiteId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteSiteId, layout, null, 2);

        var ex = await Assert.ThrowsAsync<DomainException>(
          () => fixture.Handler.Handle(command, CancellationToken.None));

        Assert.Equal(ErrorCodes.ProfileNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsError_WhenDraft_NotFound()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        var profile = TestAthletes.AGuardianControlledProfile();
        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(profile);
        fixture.SiteSnapshotRepository.GetDraftByProfileIdAsync(Arg.Any<Guid>()).ReturnsNull();

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteProfileId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteProfileId, layout, null, 2);

        var ex = await Assert.ThrowsAsync<DomainException>(
          () => fixture.Handler.Handle(command, CancellationToken.None));

        Assert.Equal(ErrorCodes.DraftConfigNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsError_WhenVersion_Conflict()
    {
        var fixture = new EditDraftAthleteSiteSnapshotFixture();

        var profile = TestAthletes.AGuardianControlledProfile();
        var snapshot = AthleteSiteSnapshots.ADraftSiteSnapshot();

        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(profile);
        fixture.SiteSnapshotRepository.GetDraftByProfileIdAsync(Arg.Any<Guid>()).Returns(snapshot);

        var layout = AthleteSiteSnapshots.DefaultLayout();
        var atheleteProfileId = Guid.NewGuid();

        var command = new EditDraftAthleteSiteSnapshotCommand(atheleteProfileId, layout, null, 2);

        var ex = await Assert.ThrowsAsync<DomainException>(
          () => fixture.Handler.Handle(command, CancellationToken.None));

        Assert.Equal(ErrorCodes.DraftVersionConflict, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsError_WhenLayoutSection_NotSupportedByTheme()
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

        var ex = await Assert.ThrowsAsync<DomainException>(
          () => fixture.Handler.Handle(command, CancellationToken.None));

        Assert.Equal(ErrorCodes.ThemeNotFound, ex.ErrorCode);
    }
}
