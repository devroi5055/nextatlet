using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Athletes.Queries;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.ValueObjects;
using NextAtlet.Domain.ValueObjects.Sections;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Characterization tests for reading and updating the draft SiteConfig via MediatR.
/// </summary>
public class DraftConfigTests
{
    private static Task<Guid> RegisterAthleteAsync(TestApp app) => app
        .Send(new RegisterOwnAthleteCommand(
            TestApp.OwnerAuthProviderId, TestApp.OwnerEmail, "Anna", "anna", new DateTime(1995, 1, 1), Locale.Da.Id))
        .ContinueWith(t => t.Result.Id);

    private static SiteLayout ValidLayout(string bioText) => new()
    {
        Sections =
        [
            new SiteSection { Id = "h", Order = 0, Data = new HeroSectionData { Headline = new LocalizedText { En = "Champion" } } },
            new SiteSection { Id = "b", Order = 1, Data = new BioSectionData { Bio = new LocalizedText { En = bioText } } }
        ]
    };

    [Fact]
    public async Task GetDraftConfig_returns_seeded_draft()
    {
        using var app = new TestApp();
        var profileId = await RegisterAthleteAsync(app);

        var dto = await app.Send(new GetDraftSiteConfigQuery(profileId));

        Assert.True(dto.IsDraft);
        Assert.Equal(1, dto.Version);
        Assert.Equal(2, dto.Layout.Sections.Count);
    }

    [Fact]
    public async Task GetDraftConfig_throws_for_unknown_profile()
    {
        using var app = new TestApp();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new GetDraftSiteConfigQuery(Guid.NewGuid())));
        Assert.Equal(ErrorCodes.DraftConfigNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateDraftConfig_bumps_version_and_sanitizes_text()
    {
        using var app = new TestApp();
        var profileId = await RegisterAthleteAsync(app);

        var result = await app.Send(new EditDraftSiteConfigCommand(profileId, ValidLayout("<p>My bio</p>"), GlobalSettings: null, ExpectedVersion: 1));

        Assert.Equal(2, result.Version);
        var bio = Assert.IsType<BioSectionData>(result.Layout.Sections[1].Data);
        Assert.Equal("My bio", bio.Bio.En); // HTML tags stripped by sanitizer
    }

    [Fact]
    public async Task UpdateDraftConfig_version_conflict_throws()
    {
        using var app = new TestApp();
        var profileId = await RegisterAthleteAsync(app);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new EditDraftSiteConfigCommand(profileId, ValidLayout("ok"), null, ExpectedVersion: 99)));
        Assert.Equal(ErrorCodes.DraftVersionConflict, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateDraftConfig_invalid_section_throws()
    {
        using var app = new TestApp();
        var profileId = await RegisterAthleteAsync(app);

        // Hero with an empty headline fails the hero validator.
        var badLayout = new SiteLayout
        {
            Sections = [new SiteSection { Id = "h", Order = 0, Data = new HeroSectionData { Headline = new LocalizedText() } }]
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            app.Send(new EditDraftSiteConfigCommand(profileId, badLayout, null, ExpectedVersion: 1)));
        Assert.Equal(ErrorCodes.SectionValidationFailed, ex.ErrorCode);
    }
}
