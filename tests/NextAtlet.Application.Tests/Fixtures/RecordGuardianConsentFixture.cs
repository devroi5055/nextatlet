using Microsoft.Extensions.Options;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Entities.Shared;
using NSubstitute;
using TestUsers = NextAtlet.Application.Tests.Shared.TestData.Users;

public class RecordGuardianConsentFixture
{
    public const string TermsVersion = "2026-01";

    public IAthleteProfileRepository AthleteRepository { get; }
    public IGuardianConsentRepository GuardianConsentRepository { get; }
    public IUserRepository UserRepository { get; }
    public IUnitOfWork UnitOfWork { get; }

    public UserProvisioner UserProvisioner { get; }

    public RecordGuardianConsentCommandHandler Handler { get; }

    public RecordGuardianConsentFixture()
    {
        AthleteRepository = Substitute.For<IAthleteProfileRepository>();
        GuardianConsentRepository = Substitute.For<IGuardianConsentRepository>();
        UserRepository = Substitute.For<IUserRepository>();
        UnitOfWork = Substitute.For<IUnitOfWork>();

        UserProvisioner = new UserProvisioner(UserRepository);

        Handler = new RecordGuardianConsentCommandHandler(
            AthleteRepository,
            GuardianConsentRepository,
            UserProvisioner,
            Options.Create(new TermsOptions { CurrentVersion = TermsVersion }),
            UnitOfWork
        );
    }

    /// <summary>Sets up an authenticated guardian the provisioner will resolve by subject.</summary>
    public User GivenAuthenticatedGuardian(string email)
    {
        var user = TestUsers.AnAuthenticatedUser();
        user.Email = email;
        UserRepository.GetByAuthProviderIdAsync(user.AuthProviderId!, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }
}
