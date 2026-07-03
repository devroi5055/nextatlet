using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Entities.Identity;

namespace NextAtlet.Application.Tests;

public class UserProvisionerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IClock          _clock = Substitute.For<IClock>();

    private UserProvisioner Build()
    {
        _clock.UtcNow.Returns(TestHelpers.UtcNow);
        return new UserProvisioner(_users, _clock);
    }

    // ── GetOrCreateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_ExistingUser_ReturnsWithoutAdding()
    {
        var existing = TestHelpers.AdultUser("auth0|exists");
        _users.GetByAuthProviderIdAsync("auth0|exists", Arg.Any<CancellationToken>())
              .Returns(existing);

        var result = await Build().GetOrCreateAsync("user@test.com", "auth0|exists", CancellationToken.None);

        Assert.Same(existing, result);
        _users.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task GetOrCreateAsync_NewUser_AddsAndReturns()
    {
        _users.GetByAuthProviderIdAsync("auth0|new", Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var result = await Build().GetOrCreateAsync("new@test.com", "auth0|new", CancellationToken.None);

        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("auth0|new",    result.AuthProviderId);
        _users.Received(1).Add(Arg.Is<User>(u => u.Email == "new@test.com"));
    }

    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        _users.GetByAuthProviderIdAsync("auth0|ghost", Arg.Any<CancellationToken>())
              .Returns((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Build().GetAsync("auth0|ghost", CancellationToken.None));
    }

    // ── TryGetAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task TryGetAsync_UserNotFound_ReturnsNull()
    {
        _users.GetByAuthProviderIdAsync("auth0|nobody", Arg.Any<CancellationToken>())
              .Returns((User?)null);

        var result = await Build().TryGetAsync("auth0|nobody", CancellationToken.None);

        Assert.Null(result);
    }
}
