using AutoFixture;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Tests.Shared;
using NextAtlet.Domain.Entities.Shared;
using NSubstitute;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Application.Interfaces.Repositories;

public class SelfRegisterAthleteFixture
{
    public ISiteRepository SiteRepository { get; }
    public IIndividualProfileRepository IndividualProfileRepository { get; }
    public ISiteLoginRepository SiteLoginRepository { get; }
    public IUserRepository UserRepository { get; }
    public IInvitationRepository InvitationRepository { get; }
    public IThemeRepository ThemeRepository { get; }
    public ISiteSnapshotRepository SiteSnapshotRepository { get;  }
    public IEmailService EmailService { get; }
    public IUnitOfWork UnitOfWork { get; }
    public IClock Clock { get; }

    public UserProvisioner UserProvisioner { get; }
    public InvitationIssuer InvitationIssuer { get; }
    public AgeThresholdOptions AgeThresholds { get; }

    public SelfRegisterAthleteCommandHandler Handler { get; }

    // Defaults to the DK launch thresholds; pass a custom set (e.g. SelfConsentAge = 16) to exercise
    // the guardian-consent path, which is dormant when SelfConsentAge == AbsoluteMinimumAge.
    public SelfRegisterAthleteFixture()
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
        InvitationRepository = Substitute.For<IInvitationRepository>();
        EmailService = Substitute.For<IEmailService>();
        UnitOfWork = Substitute.For<IUnitOfWork>();

        UserProvisioner = new UserProvisioner(UserRepository, Clock);

        InvitationIssuer = new InvitationIssuer(
            InvitationRepository,
            EmailService,
            Options.Create(new InvitationOptions { ExpiryDays = 7 })
        );

        ThemeRepository.GetActiveByNameAsync("Classic", CancellationToken.None)
            .Returns(fixture.Create<Theme>());

        Handler = new SelfRegisterAthleteCommandHandler(
            SiteRepository,
            SiteLoginRepository,
            IndividualProfileRepository,
            ThemeRepository,
            SiteSnapshotRepository,
            UserProvisioner,
            InvitationIssuer,
            Clock,
            Options.Create(AgeThresholds),
            EmailService,
            UnitOfWork
        );
    }
}
