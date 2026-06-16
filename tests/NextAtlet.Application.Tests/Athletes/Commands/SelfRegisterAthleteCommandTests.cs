using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Shared;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NextAtlet.Application.Tests.Athletes.Commands
{
    public class SelfRegisterAthleteCommandTests
    {

        private static SelfRegisterAthleteCommand CreateCommand(DateTime dob, string? guardianEmail)
        {
            return new SelfRegisterAthleteCommand(
                "auth0|123",
                "john@test.com",
                "john",
                "john",
                dob,
                Locale.Da.Id,
                guardianEmail);
        }
        [Theory]
        [InlineData(12)]
        [InlineData(11)]
        [InlineData(5)]
        public async Task Fails_When_BelowMinimumAge(int age)
        {
            var fixture = new SelfRegisterAthleteFixture();

            var dob = fixture.Clock.UtcNow.AddYears(-age);

            var command = CreateCommand(dob, guardianEmail: null);

            var result = await fixture.Handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.BelowMinimumAge, result.Error!.Code);
        }

        [Theory]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        public async Task RequiresGuardianEmail_ForYoungMinor(int age)
        {
            var fixture = new SelfRegisterAthleteFixture();

            var dob = fixture.Clock.UtcNow.AddYears(-age);

            var commandFail = CreateCommand(
                dob,
                guardianEmail: null);
            var commandSuccess = CreateCommand(
                dob,
                guardianEmail: "john@john.com");

            //fails without guardian email
            var failure = await fixture.Handler.Handle(commandFail, CancellationToken.None);
            Assert.False(failure.IsSuccess);
            Assert.Equal(ErrorCodes.GuardianEmailRequired, failure.Error!.Code);

            //success with guardian email
            var success = await fixture.Handler.Handle(
                commandSuccess, CancellationToken.None);

            Assert.True(success.IsSuccess);
        }

        [Theory]
        [InlineData(16)]
        [InlineData(17)]
        public async Task DoesNotRequireGuardianEmail_ForOlderMinor(int age)
        {
            var fixture = new SelfRegisterAthleteFixture();

            var dob = fixture.Clock.UtcNow.AddYears(-age);

            var command = CreateCommand(
                dob,
                guardianEmail: null);

            var result = await fixture.Handler.Handle(
                command,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(18)]
        [InlineData(25)]
        [InlineData(40)]
        public async Task DoesNotRequireGuardianEmail_ForAdult(int age)
        {
            var fixture = new SelfRegisterAthleteFixture();

            var dob = fixture.Clock.UtcNow.AddYears(-age);

            var command = CreateCommand(
                dob,
                guardianEmail: null);

            var result = await fixture.Handler.Handle(
                command,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Fails_WhenSlug_ExistsAlready()
        {
            var fixture = new SelfRegisterAthleteFixture();

            // Adult so the consent gate doesn't short-circuit before the slug check.
            var dob = fixture.Clock.UtcNow.AddYears(-25);
            var slug = "john-doe";

            fixture.SiteRepository.SlugExistsAsync(slug).Returns(true);

            var command = new SelfRegisterAthleteCommand(
                AuthProviderId: "auth0|123",
                Email: "test@test.com",
                DisplayName: "Lucas",
                Slug: slug,
                DateOfBirth: dob,
                DefaultLocaleId: "en",
                GuardianEmail: null
            );

            var result = await fixture.Handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.SlugAlreadyTaken, result.Error!.Code);
        }

        [Fact]
        public async Task ReturnsDTO_When_success()
        {
            var fixture = new SelfRegisterAthleteFixture();

            //13y old
            var dob = new DateTime(2003, 1, 1);
            var authProviderId = "auth0|123";
            var email = "test@test.com";
            var diplayName = "Lucas";
            var slug = "lucas";
            var guardianEmail = "guardian@guardian.com";

            var command = new SelfRegisterAthleteCommand(
                authProviderId,
                email,
                diplayName,
                slug,
                DateOfBirth: dob,
                Locale.Da.Id,
                guardianEmail
            );


            var result = await fixture.Handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var dto = result.Value!;

            // SiteDto carries the site identity; age/control live on AthleteProfile, not this DTO.
            Assert.Equal(slug, dto.Slug);
            Assert.Equal(diplayName, dto.DisplayName);
            Assert.Same(Locale.Da.Id, dto.DefaultLocale.Id);
        }

        [Fact]
        public async Task CreatesUser_WhenUser_DoesntExist()
        {
            var fixture = new SelfRegisterAthleteFixture();

            //13y old
            var authProviderId = "auth0|123";
            var dob = new DateTime(2003, 1, 1);
            var email = "test@test.com";
            var diplayName = "Lucas";
            var slug = "lucas";
            var guardianEmail = "guardian@guardian.com";

            var command = new SelfRegisterAthleteCommand(
                authProviderId,
                email,
                diplayName,
                slug,
                dob,
                Locale.Da.Id,
                guardianEmail
            );

            await fixture.Handler.Handle(command, CancellationToken.None);
            fixture.UserRepository.Received(1).Add(Arg.Any<User>());
            await fixture.UserRepository.Received(1).GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FetchUser_WhenUser_Exist()
        {
            var fixture = new SelfRegisterAthleteFixture();

            //13y old
            var authProviderId = "auth0|123";
            var dob = new DateTime(2003, 1, 1);
            var email = "test@test.com";
            var diplayName = "Lucas";
            var slug = "lucas";
            var guardianEmail = "guardian@guardian.com";

            fixture.UserRepository.GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>()).Returns(Users.AnAuthenticatedUser(authProviderId));

            var command = new SelfRegisterAthleteCommand(
                authProviderId,
                email,
                diplayName,
                slug,
                dob,
                Locale.Da.Id,
                guardianEmail
            );

            await fixture.Handler.Handle(command, CancellationToken.None);
            fixture.UserRepository.Received(0).Add(Arg.Any<User>());
            await fixture.UserRepository.Received(1).GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Fails_WhenUser_HaveExistingProfile()
        {
            var fixture = new SelfRegisterAthleteFixture();

            //13y old
            var authProviderId = "auth0|123";
            var dob = new DateTime(2003, 1, 1);
            var email = "test@test.com";
            var diplayName = "Lucas";
            var slug = "lucas";
            var guardianEmail = "guardian@guardian.com";

            var fakeUser = Users.AnAuthenticatedUser(authProviderId);
            var existingSite = new Site { Slug = "existing", DisplayName = "Existing" };

            fixture.UserRepository.GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>()).Returns(fakeUser);
            fixture.SiteRepository.GetOwnedByUserIdAsync(fakeUser.Id, Arg.Any<CancellationToken>()).Returns(existingSite);

            var command = new SelfRegisterAthleteCommand(
                authProviderId,
                email,
                diplayName,
                slug,
                dob,
                Locale.Da.Id,
                guardianEmail
            );

            var result = await fixture.Handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.SiteAlreadyExists, result.Error!.Code);

            fixture.AthleteProfileRepository.Received(0).Add(Arg.Any<AthleteProfile>());
            fixture.SiteSnapshotRepository.Received(0).Add(Arg.Any<SiteSnapshot>());
            fixture.SiteLoginRepository.Received(0).Add(Arg.Any<SiteLogin>());
        }

        [Fact]
        public async Task CreateProfile_WhenUser_IsValidated()
        {
            var fixture = new SelfRegisterAthleteFixture();

            //13y old
            var authProviderId = "auth0|123";
            var dob = new DateTime(2003, 1, 1);
            var email = "test@test.com";
            var diplayName = "Lucas";
            var slug = "lucas";
            var guardianEmail = "guardian@guardian.com";

            var fakeUser = Users.AnAuthenticatedUser(authProviderId);

            fixture.UserRepository.GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>()).Returns(fakeUser);
            fixture.SiteRepository.GetOwnedByUserIdAsync(fakeUser.Id, Arg.Any<CancellationToken>()).ReturnsNull();

            var command = new SelfRegisterAthleteCommand(
                authProviderId,
                email,
                diplayName,
                slug,
                dob,
                Locale.Da.Id,
                guardianEmail
            );

            await fixture.Handler.Handle(command, CancellationToken.None);

            fixture.AthleteProfileRepository.Received(1).Add(Arg.Any<AthleteProfile>());
            fixture.SiteSnapshotRepository.Received(1).Add(Arg.Any<SiteSnapshot>());
            fixture.SiteLoginRepository.Received(1).Add(Arg.Any<SiteLogin>());
        }
    }
}
