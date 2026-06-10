using AutoFixture;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Tests.Shared;
using NextAtlet.Domain.Entities.Shared;
using NSubstitute;

public class EditDraftAthleteSiteSnapshotFixture
{
    public IAthleteSiteRepository AthleteRepository { get; }
    public IInvitationRepository InvitationRepository { get; }
    public IThemeRepository ThemeRepository { get; }
    public IAthleteSiteSnapshotRepository SiteSnapshotRepository { get; }
    public ISanitizationService SanitizationService { get; }
    public ISectionTypeRegistry SectionRegistry { get; }

    public IUnitOfWork UnitOfWork { get; }

    public EditDraftAthleteSiteSnapshotCommandHandler Handler { get; }

    public EditDraftAthleteSiteSnapshotFixture()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new SectionDataSpecimentBuilder());
        fixture.Register<DateOnly>(() =>
        {
            var year = Random.Shared.Next(1990, 2010);
            return new DateOnly(year, 1, 1);
        });
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        AthleteRepository = Substitute.For<IAthleteSiteRepository>();
        SiteSnapshotRepository = Substitute.For<IAthleteSiteSnapshotRepository>();
        ThemeRepository = Substitute.For<IThemeRepository>();
        SectionRegistry = Substitute.For<ISectionTypeRegistry>();
        SanitizationService = Substitute.For<ISanitizationService>();
        InvitationRepository = Substitute.For<IInvitationRepository>();
        UnitOfWork = Substitute.For<IUnitOfWork>();

        ThemeRepository.GetActiveByNameAsync("Classic", CancellationToken.None)
            .Returns(fixture.Create<Theme>());

        Handler = new EditDraftAthleteSiteSnapshotCommandHandler(
            AthleteRepository,
            SiteSnapshotRepository,
            ThemeRepository,
            SectionRegistry,
            SanitizationService,
            UnitOfWork
        );
    }
}
