using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
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
        public async Task Throws_When_BelowMinimumAge(int age)
        {
            var fixture = new SelfRegisterAthleteFixture();

            var dob = fixture.Clock.UtcNow.AddYears(-age);

            var command = CreateCommand(dob, guardianEmail: null);

            await Assert.ThrowsAsync<DomainException>(
                () => fixture.Handler.Handle(command, CancellationToken.None));
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
            await Assert.ThrowsAsync<DomainException>(
                () => fixture.Handler.Handle(commandFail, CancellationToken.None));

            //success with guardian email
            var dto = await fixture.Handler.Handle(
                commandSuccess, CancellationToken.None);

            Assert.NotNull(dto);
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

            var dto = await fixture.Handler.Handle(
                command,
                CancellationToken.None);

            Assert.NotNull(dto);
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

            var dto = await fixture.Handler.Handle(
                command,
                CancellationToken.None);

            Assert.NotNull(dto);
        }

        [Fact]
        public async Task ThrowsError_WhenSlug_ExistsAlready()
        {
            var fixture = new SelfRegisterAthleteFixture();

            var dob = fixture.Clock.UtcNow.AddYears(-15);
            var slug = "john-doe";

            fixture.AthleteRepository.SlugExistsAsync(slug).Returns(true);

            var command = new SelfRegisterAthleteCommand(
                AuthProviderId: "auth0|123",
                Email: "test@test.com",
                DisplayName: "Lucas",
                Slug: slug,
                DateOfBirth: dob,
                DefaultLocaleId: "en",
                GuardianEmail: null
            );

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

            var dto = fixture.Handler.Handle(command, CancellationToken.None);
            Assert.Equal(slug, dto.Result.Slug);
            Assert.Equal(diplayName, dto.Result.DisplayName);
            Assert.Equal(DateOnly.FromDateTime(dob), dto.Result.DateOfBirth);
            Assert.True(dto.Result.IsMinor);
            Assert.Equal(ControlMode.AthleteControlled, dto.Result.ControlMode);
            Assert.Same(Locale.Da.Id, dto.Result.DefaultLocale.Id);

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
        public async Task ThrowError_WhenUser_HaveExistingProfile()
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
            var fakeProfile = TestAthletes.AnAthlete();

            fixture.UserRepository.GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>()).Returns(fakeUser);
            fixture.AthleteRepository.GetOwnedByUserIdAsync(fakeUser.Id, Arg.Any<CancellationToken>()).Returns(fakeProfile);

            var command = new SelfRegisterAthleteCommand(
                authProviderId,
                email,
                diplayName,
                slug,
                dob,
                Locale.Da.Id,
                guardianEmail
            );

            await Assert.ThrowsAsync<DomainException>(
                () => fixture.Handler.Handle(command, CancellationToken.None));

            fixture.AthleteRepository.Received(0).Add(Arg.Any<AthleteProfile>());
            fixture.SiteConfigRepository.Received(0).Add(Arg.Any<SiteConfig>());
            fixture.ProfileLoginRepository.Received(0).Add(Arg.Any<ProfileLogin>());
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
            var fakeProfile = TestAthletes.AnAthlete();

            fixture.UserRepository.GetByAuthProviderIdAsync(authProviderId, Arg.Any<CancellationToken>()).Returns(fakeUser);
            fixture.AthleteRepository.GetOwnedByUserIdAsync(fakeUser.Id, Arg.Any<CancellationToken>()).ReturnsNull();

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

            fixture.AthleteRepository.Received(1).Add(Arg.Any<AthleteProfile>());
            fixture.SiteConfigRepository.Received(1).Add(Arg.Any<SiteConfig>());
            fixture.ProfileLoginRepository.Received(1).Add(Arg.Any<ProfileLogin>());
        }
    }
}
