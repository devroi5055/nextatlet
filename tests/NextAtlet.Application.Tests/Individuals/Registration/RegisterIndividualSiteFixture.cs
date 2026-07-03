using AutoFixture;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Application.Tests.Shared;
using NextAtlet.Domain.Entities.Sites;
using NSubstitute;

public class RegisterIndividualSiteSelfFixture
{
    public const string TermsVersion = "2026-01";

    public ISiteRepository SiteRepository { get; }
    public IIndividualProfileRepository IndividualProfileRepository { get; }
    public ISiteLoginRepository SiteLoginRepository { get; }
    public IUserRepository UserRepository { get; }
    public IActionTokenRepository ActionTokenRepository { get; }
    public IThemeRepository ThemeRepository { get; }
    public ISiteSnapshotRepository SiteSnapshotRepository { get; }
    public IEmailService EmailService { get; }
    public IUnitOfWork UnitOfWork { get; }
    public IClock Clock { get; }

    public UserProvisioner UserProvisioner { get; }
    public AgeThresholdOptions AgeThresholds { get; }

    public RegisterIndividualSiteSelfCommandHandler Handler { get; }

    // Defaults to the DK launch thresholds (self-consent age 16), so the 13–15 band requires guardian
    // consent and a Consent action token is staged + emailed.
    public RegisterIndividualSiteSelfFixture()
    {
        AgeThresholds = new AgeThresholdOptions();

        var fixture = new Fixture();
        fixture.Customizations.Add(new SectionDataSpecimentBuilder());
        fixture.Register<DateOnly>(() =>
        {
            var year = Random.Shared.Next(1990, 2010);
            return new DateOnly(year, 1, 1);
        });
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());


        //makes today 2018
        Clock = MockFactory.CreateClock(new DateTime(2018, 1, 1));

        SiteRepository = Substitute.For<ISiteRepository>();
        IndividualProfileRepository = Substitute.For<IIndividualProfileRepository>();
        SiteLoginRepository = Substitute.For<ISiteLoginRepository>();
        ThemeRepository = Substitute.For<IThemeRepository>();
        SiteSnapshotRepository = Substitute.For<ISiteSnapshotRepository>();
        UserRepository = Substitute.For<IUserRepository>();
        ActionTokenRepository = Substitute.For<IActionTokenRepository>();
        EmailService = Substitute.For<IEmailService>();
        UnitOfWork = Substitute.For<IUnitOfWork>();

        UserProvisioner = new UserProvisioner(UserRepository, Clock);

        ThemeRepository.GetActiveByNameAsync("Classic", CancellationToken.None)
            .Returns(fixture.Create<Theme>());

        Handler = new RegisterIndividualSiteSelfCommandHandler(
            SiteRepository,
            SiteLoginRepository,
            IndividualProfileRepository,
            ThemeRepository,
            SiteSnapshotRepository,
            UserProvisioner,
            Clock,
            Options.Create(AgeThresholds),
            EmailService,
            ActionTokenRepository,
            Options.Create(new TermsOptions { CurrentVersion = TermsVersion }),
            Options.Create(new InvitationOptions { ExpiryDays = 7 }),
            UnitOfWork
        );
    }
}
