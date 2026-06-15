using AutoFixture;
using NextAtlet.Domain.Entities.Shared;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// Test instances of <see cref="User"/>. The authenticated/pending distinction is load-bearing in the
/// registration + invitation flow (a pending user is an invitee with no IdP subject yet).
/// </summary>
public static class Users
{
    public static User AnAuthenticatedUser(string? authProviderId = null, Action<User>? customize = null)
    {
        var user = TestFixture.Create().Build<User>()
            .Without(u => u.ProfileLogins)
            .With(u => u.Email, $"user-{Guid.NewGuid():N}@test.local")
            .With(u => u.AuthProviderId, authProviderId ?? $"auth0|{Guid.NewGuid():N}")
            .Create();
        customize?.Invoke(user);
        return user;
    }

    public static User APendingUser(string? email = null, Action<User>? customize = null)
    {
        var user = TestFixture.Create().Build<User>()
            .Without(u => u.ProfileLogins)
            .With(u => u.Email, email ?? $"invited-{Guid.NewGuid():N}@test.local")
            .Without(u => u.AuthProviderId) // invited, not yet claimed → null
            .Create();
        customize?.Invoke(user);
        return user;
    }
}
