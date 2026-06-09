using FluentAssertions;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations;
using NextAtlet.Domain.Enumerations.Enums.AthleteProfile;
using System.Globalization;
using Xunit;

namespace NextAtlet.Domain.Tests.Entities;

public class InvitationTests
{
    private static Invitation APendingInvitation(string roleId = "guardian", int expiresInDays = 7) => new()
    {
        TargetProfileId = Guid.NewGuid(),
        RoleId = roleId,
        Email = "parent@example.com",
        Status = InvitationStatus.Pending,
        ExpiresUtc = DateTime.UtcNow.AddDays(expiresInDays),
        InvitedByUserId = Guid.NewGuid()
    };

    [Fact]
    public void IdServesAsTheAcceptanceKey()
    {
        var invitation = APendingInvitation();

        invitation.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewInvitation_StartsPending()
    {
        var invitation = APendingInvitation();

        invitation.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void NewInvitation_HasNoAcceptedTimestamp()
    {
        var invitation = APendingInvitation();

        invitation.AcceptedUtc.Should().BeNull();
    }

    [Fact(Skip = "Confirm whether IsExpired is an entity property or computed in the handler.")]
    public void IsExpired_WhenPastExpiry_IsTrue()
    {
        var invitation = APendingInvitation(expiresInDays: -1);

        invitation.IsExpired.Should().BeTrue();
    }

    [Fact(Skip = "Confirm whether IsExpired is an entity property or computed in the handler.")]
    public void IsExpired_WhenWithinWindow_IsFalse()
    {
        var invitation = APendingInvitation(expiresInDays: 7);

        invitation.IsExpired.Should().BeFalse();
    }
}

    