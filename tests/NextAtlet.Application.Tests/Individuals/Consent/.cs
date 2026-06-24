using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Application.Features.Individuals.Consent;
using NextAtlet.Application.Tests.Shared.TestData;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Domain.Enumerations.Individual;
using NSubstitute;

namespace NextAtlet.Application.Tests.Individuals.Consent;

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
        var profile = TestIndividuals.APendingGuardianConsentAthlete();
        fixture.AthleteRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await fixture.Handler.Handle(
            new RecordGuardianConsentCommand(profile.Id, guardian.AuthProviderId!, guardian.Email), CancellationToken.None);

        // Recorded consent → success carrying the new consent id.
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        // CreatedUtc is stamped during SaveChangesAsync (mocked here), so it isn't asserted at Add time.
        fixture.GuardianConsentRepository.Received(1).Add(Arg.Is<GuardianConsent>(c =>
            c.IndividualProfileId == profile.Id &&
            c.GuardianUserId == guardian.Id &&
            c.MethodId == ConsentMethods.VerifiedEmail.Id &&
            c.TermsVersion == RecordGuardianConsentFixture.TermsVersion));
        Assert.Equal(ConsentStates.Consented.Id, profile.ConsentStateId);
    }

    [Fact]
    public async Task ProfileNotAwaitingConsent_IsANoOp()
    {
        var fixture = new RecordGuardianConsentFixture();
        var guardian = fixture.GivenAuthenticatedGuardian("guardian@test.local");
        var profile = TestIndividuals.AnAthlete(); // ConsentState NotRequired
        fixture.AthleteRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await fixture.Handler.Handle(
            new RecordGuardianConsentCommand(profile.Id, guardian.AuthProviderId!, guardian.Email), CancellationToken.None);

        // Consent not needed → empty success (nothing recorded, gate unchanged).
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        fixture.GuardianConsentRepository.DidNotReceive().Add(Arg.Any<GuardianConsent>());
        Assert.Equal(ConsentStates.NotRequired.Id, profile.ConsentStateId);
    }

    [Fact]
    public async Task UnknownProfile_IsRejected()
    {
        var fixture = new RecordGuardianConsentFixture();
        fixture.AthleteRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IndividualProfile?)null);

        var result = await fixture.Handler.Handle(
            new RecordGuardianConsentCommand(Guid.NewGuid(), "auth0|x", "g@test.local"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.IndividualProfileNotFound, result.Error!.Code);
    }
}
