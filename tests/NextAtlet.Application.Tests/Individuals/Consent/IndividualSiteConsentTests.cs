using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;
using NSubstitute;

namespace NextAtlet.Application.Tests.Individuals.Consent;

/// <summary>
/// Guardian-consent behaviour of self-registration. Under Denmark's defaults (self-consent age 13)
/// the consent path is dormant; these raise SelfConsentAge to 16 so the 13–15 band requires consent.
/// </summary>
public class SelfRegisterConsentTests
{

    private static RegisterIndividualSiteSelfCommand Command(DateTime dob, string? guardianEmail)
        => new("auth0|123", "athlete@test.com", "Kid", "kid", dob, Locale.Da.Id, guardianEmail);

    [Fact]
    public async Task ConsentBand_CreatesPendingGuardianConsent_AndSendsConsentEmail_NoInvitation()
    {
        var fixture = new RegisterIndividualSiteSelfFixture();
        var dob = fixture.Clock.UtcNow.AddYears(-14); // below self-consent age 16

        await fixture.Handler.Handle(Command(dob, "guardian@test.com"), CancellationToken.None);

        // Profile is publish-gated...
        fixture.IndividualProfileRepository.Received(1)
            .Add(Arg.Is<IndividualProfile>(p => p.ConsentStateId == ConsentStates.PendingGuardianConsent.Id));
        // ...and the guardian gets a consent-request EMAIL (not a profile invitation).
        await fixture.EmailService.Received(1).SendConsentRequestAsync("guardian@test.com", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        fixture.InvitationRepository.DidNotReceive().Add(Arg.Any<Invitation>());
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
    public async Task AtSelfConsentAge_CreatesNotRequired_AndNoConsentInvitation()
    {
        var fixture = new RegisterIndividualSiteSelfFixture();
        var dob = fixture.Clock.UtcNow.AddYears(-16); // exactly self-consent age → no consent needed

        await fixture.Handler.Handle(Command(dob, guardianEmail: null), CancellationToken.None);

        fixture.IndividualProfileRepository.Received(1)
            .Add(Arg.Is<IndividualProfile>(p => p.ConsentStateId == ConsentStates.NotRequired.Id));
        fixture.InvitationRepository.DidNotReceive().Add(Arg.Any<Invitation>());
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
