using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Application.Features.Organizations.Verification;
using NextAtlet.Domain.Entities.ClubRegistry;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Application.Tests;

public class SendOfficialEmailVerificationHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly IClubRepository        _clubs  = Substitute.For<IClubRepository>();
    private readonly IActionTokenRepository _tokens = Substitute.For<IActionTokenRepository>();
    private readonly IEmailService          _email  = Substitute.For<IEmailService>();
    private readonly IUnitOfWork            _uow    = Substitute.For<IUnitOfWork>();
    private readonly IClock                 _clock  = Substitute.For<IClock>();
    private readonly IUserRepository        _users  = Substitute.For<IUserRepository>();

    private SendOfficialEmailVerificationCommandHandler BuildHandler()
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        var opts        = Options.Create(new InvitationOptions { ExpiryDays = 7 });
        var provisioner = new UserProvisioner(_users, _clock);
        return new SendOfficialEmailVerificationCommandHandler(
            _clubs, _tokens, _email, _uow, _clock, opts, provisioner);
    }

    private static ClubOfficial OfficialWithEmail(string email = "chair@club.dk") => new()
    {
        ClubId = Guid.NewGuid(),
        Name   = "Test Official",
        Email  = email,
        Phone  = null,
        RoleId = ClubOfficialRole.Chairman.Id
    };

    private SendOfficialEmailVerificationCommand Command() =>
        new("auth0|org", "org@test.com", Guid.NewGuid(), Guid.NewGuid());

    // ── Sad paths ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_OfficialNotFound_ReturnsVerificationOfficialNotFoundError()
    {
        _clubs.GetOfficialByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns((ClubOfficial?)null);

        var result = await BuildHandler().Handle(Command(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VerificationOfficialNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_OfficialHasNoEmail_ReturnsVerificationOfficialEmailMissingError()
    {
        var official = new ClubOfficial
        {
            ClubId = Guid.NewGuid(),
            Name   = "No Email",
            Email  = null,
            Phone  = null,
            RoleId = ClubOfficialRole.Other.Id
        };
        _clubs.GetOfficialByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns(official);

        var result = await BuildHandler().Handle(Command(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VerificationOfficialEmailMissing, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_OfficialHasWhitespaceEmail_ReturnsVerificationOfficialEmailMissingError()
    {
        var official = new ClubOfficial
        {
            ClubId = Guid.NewGuid(),
            Name   = "Whitespace Email",
            Email  = "   ",
            Phone  = null,
            RoleId = ClubOfficialRole.Other.Id
        };
        _clubs.GetOfficialByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns(official);

        var result = await BuildHandler().Handle(Command(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.VerificationOfficialEmailMissing, result.Error!.Code);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidOfficial_IssuesTokenAndEmailsRegistryAddress()
    {
        var official = OfficialWithEmail("chair@club.dk");
        _clubs.GetOfficialByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
              .Returns(official);
        // Caller may not have a User row yet — that's fine.
        _users.GetByAuthProviderIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var result = await BuildHandler().Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _tokens.Received(1).Add(Arg.Any<ActionToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        // Email goes to the registry address, not the caller-supplied address.
        await _email.Received(1).SendOrgVerificationAsync(
            "chair@club.dk", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
