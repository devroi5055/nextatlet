using NextAtlet.Application.Features.Account.Queries;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Domain.Enumerations;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Tests for the /me domain-gate query (GetCurrentUserQuery) via the MediatR pipeline.
/// Identity is supplied by the fake ICurrentUserContext.
/// </summary>
public class AccountQueryTests
{
    [Fact]
    public async Task Me_reports_unregistered_when_caller_has_no_user()
    {
        using var app = new TestApp();

        var me = await app.Send(new GetCurrentUserQuery());

        Assert.False(me.Registered);
        Assert.Null(me.Role);
    }

    [Fact]
    public async Task Me_reports_owner_after_registration()
    {
        using var app = new TestApp();
        await app.Send(new RegisterAthleteProfileCommand("Anna", "anna", new DateTime(1995, 1, 1), Locale.Da.Id));

        var me = await app.Send(new GetCurrentUserQuery());

        Assert.True(me.Registered);
        Assert.Equal(ProfileRole.AthleteOwner.Id, me.Role);
    }
}
