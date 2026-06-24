using FluentAssertions;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Domain.Tests.Entities.Identity;

/// <summary>
/// Lifecycle of the single-use, expiring <see cref="ActionToken"/> — the row Id is the link key, and
/// completion is recorded by <see cref="ActionToken.AcceptedUtc"/> (no Status enum). Replaces the old
/// Invitation entity tests.
/// </summary>
public class ActionTokenTests
{
    private static ActionToken APendingToken(int expiresInDays = 7) =>
        ActionToken.Issue(
            ActionTokenType.Invitation.Id,
            Guid.NewGuid(),
            new InvitePayload { Email = "parent@example.com", RoleId = IndividualRole.Guardian.Id },
            DateTime.UtcNow.AddDays(expiresInDays));

    [Fact]
    public void IdServesAsTheAcceptanceKey()
        => APendingToken().Id.Should().NotBe(Guid.Empty);

    [Fact]
    public void NewToken_IsPending()
        => APendingToken().IsPending.Should().BeTrue();

    [Fact]
    public void NewToken_HasNoAcceptedTimestamp()
        => APendingToken().AcceptedUtc.Should().BeNull();

    [Fact]
    public void Accept_StampsAcceptedUtc_AndEndsPending()
    {
        var token = APendingToken();
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        token.Accept(now);

        token.AcceptedUtc.Should().Be(now);
        token.IsPending.Should().BeFalse();
    }

    [Fact]
    public void Accept_AlreadyAccepted_Throws()
    {
        var token = APendingToken();
        token.Accept(DateTime.UtcNow);

        var act = () => token.Accept(DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsExpired_PastExpiryWhilePending_IsTrue()
        => APendingToken(expiresInDays: -1).IsExpired.Should().BeTrue();

    [Fact]
    public void IsExpired_WithinWindow_IsFalse()
        => APendingToken(expiresInDays: 7).IsExpired.Should().BeFalse();

    [Fact]
    public void IsExpired_Accepted_IsFalse_EvenIfPastExpiry()
    {
        // A completed token is never "expired" — its durable outcome is already recorded.
        var token = ActionToken.Issue(
            ActionTokenType.Consent.Id, Guid.NewGuid(),
            new ConsentPayload { Email = "g@example.com", TermsVersion = "2026-01" },
            DateTime.UtcNow.AddSeconds(-1));

        token.Accept(DateTime.UtcNow);

        token.IsExpired.Should().BeFalse();
    }
}
