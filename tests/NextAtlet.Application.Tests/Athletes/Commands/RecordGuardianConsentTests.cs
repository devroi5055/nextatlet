using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using NSubstitute;

namespace NextAtlet.Application.Tests.Athletes.Commands;

/// <summary>
/// The consent endpoint records a GuardianConsent (the four GDPR facts) and lifts the publish gate.
/// It never creates a ProfileLogin — consent is not joining.
/// </summary>
public class RecordGuardianConsentTests
{
    [Fact]
    public async Task PendingProfile_RecordsConsent_AndLiftsTheGate()
    {
        var fixture = new RecordGuardianConsentFixture();
        var guardian = fixture.GivenAuthenticatedGuardian("guardian@test.local");
        var profile = TestAthletes.APendingGuardianConsentAthlete();
        fixture.AthleteRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        await fixture.Handler.Handle(
            new RecordGuardianConsentCommand(profile.Id, guardian.AuthProviderId!, guardian.Email), CancellationToken.None);

        fixture.GuardianConsentRepository.Received(1).Add(Arg.Is<GuardianConsent>(c =>
            c.AthleteProfileId == profile.Id &&
            c.GuardianUserId == guardian.Id &&
            c.Method == ConsentMethod.VerifiedEmail &&
            c.TermsVersion == RecordGuardianConsentFixture.TermsVersion &&
            c.ConsentedUtc != default));
        Assert.Equal(ConsentState.Consented, profile.ConsentState);
    }

    [Fact]
    public async Task ProfileNotAwaitingConsent_IsANoOp()
    {
        var fixture = new RecordGuardianConsentFixture();
        var guardian = fixture.GivenAuthenticatedGuardian("guardian@test.local");
        var profile = TestAthletes.AnAthlete(); // ConsentState NotRequired
        fixture.AthleteRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        await fixture.Handler.Handle(
            new RecordGuardianConsentCommand(profile.Id, guardian.AuthProviderId!, guardian.Email), CancellationToken.None);

        fixture.GuardianConsentRepository.DidNotReceive().Add(Arg.Any<GuardianConsent>());
        Assert.Equal(ConsentState.NotRequired, profile.ConsentState);
    }

    [Fact]
    public async Task UnknownProfile_IsRejected()
    {
        var fixture = new RecordGuardianConsentFixture();
        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AthleteProfile?)null);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            fixture.Handler.Handle(new RecordGuardianConsentCommand(Guid.NewGuid(), "auth0|x", "g@test.local"), CancellationToken.None));
        Assert.Equal(ErrorCodes.ProfileNotFound, ex.ErrorCode);
    }
}
