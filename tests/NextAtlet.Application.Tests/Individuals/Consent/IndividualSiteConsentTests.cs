using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;
using NSubstitute;

namespace NextAtlet.Application.Tests.Individuals.Consent;

/// <summary>
/// Guardian-consent behaviour of self-registration. Under the DK launch defaults (self-consent age 16)
/// the 13–15 band requires consent: the profile is publish-gated and a Consent <see cref="ActionToken"/>
/// is staged + emailed to the guardian (consent is its own link-bearing flow, not a profile-join invite).
/// </summary>
public class SelfRegisterConsentTests
{
    private static RegisterIndividualSiteSelfCommand Command(DateTime dob, string? guardianEmail)
        => new("auth0|123", "athlete@test.com", "Kid", "kid", dob, Locale.Da.Id, guardianEmail);

    [Fact]
    public async Task ConsentBand_CreatesPendingGuardianConsent_AndSendsConsentEmail_ViaConsentToken()
    {
        var fixture = new RegisterIndividualSiteSelfFixture();
        var dob = fixture.Clock.UtcNow.AddYears(-14); // below self-consent age 16

        await fixture.Handler.Handle(Command(dob, "guardian@test.com"), CancellationToken.None);

        // Profile is publish-gated...
        fixture.IndividualProfileRepository.Received(1)
            .Add(Arg.Is<IndividualProfile>(p => p.ConsentStateId == ConsentStates.PendingGuardianConsent.Id));
        // ...the guardian gets a consent-request EMAIL, staged as a Consent action token (not a join invite).
        fixture.ActionTokenRepository.Received(1)
            .Add(Arg.Is<ActionToken>(t => t.TypeId == ActionTokenType.Consent.Id));
        await fixture.EmailService.Received(1)
            .SendConsentRequestAsync("guardian@test.com", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConsentBand_WithoutGuardianEmail_IsRejected()
    {
        var fixture = new RegisterIndividualSiteSelfFixture();
        var dob = fixture.Clock.UtcNow.AddYears(-14);

        var result = await fixture.Handler.Handle(Command(dob, guardianEmail: null), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.GuardianEmailRequired, result.Error!.Code);
    }

    [Fact]
    public async Task AtSelfConsentAge_CreatesNotRequired_AndNoConsentToken()
    {
        var fixture = new RegisterIndividualSiteSelfFixture();
        var dob = fixture.Clock.UtcNow.AddYears(-16); // exactly self-consent age → no consent needed

        await fixture.Handler.Handle(Command(dob, guardianEmail: null), CancellationToken.None);

        fixture.IndividualProfileRepository.Received(1)
            .Add(Arg.Is<IndividualProfile>(p => p.ConsentStateId == ConsentStates.NotRequired.Id));
        fixture.ActionTokenRepository.DidNotReceive().Add(Arg.Any<ActionToken>());
    }

    [Fact]
    public async Task BelowAbsoluteMinimum_IsRejected_EvenWhenConsentConfigured()
    {
        var fixture = new RegisterIndividualSiteSelfFixture();
        var dob = fixture.Clock.UtcNow.AddYears(-10); // below AbsoluteMinimumAge 13 → cannot register

        var result = await fixture.Handler.Handle(Command(dob, "guardian@test.com"), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BelowMinimumAge, result.Error!.Code);
    }
}
