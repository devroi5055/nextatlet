using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NextAtlet.Application;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Infrastructure.Data;
using NextAtlet.Infrastructure.Persistence;
using NextAtlet.Infrastructure.Persistence.Repositories;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;

namespace NextAtlet.Application.Tests;

/// <summary>
/// In-memory composition root mirroring Program.cs (MediatR + repositories + UoW + services),
/// backed by EF InMemory instead of Postgres. Each instance gets an isolated, seeded database
/// and dispatches requests through the real ISender pipeline — so the tests exercise the
/// MediatR wiring, not handler internals.
/// </summary>
internal sealed class TestApp : IDisposable
{
    public const string OwnerAuthProviderId = "test-auth-sub";
    public const string OwnerEmail = "owner@test.local";

    private readonly ServiceProvider _provider;

    public TestApp()
    {
        var dbName = $"nextatlet-tests-{Guid.NewGuid()}"; // shared across scopes within this app
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<NextAtletDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IApplicationMarker).Assembly));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAthleteProfileRepository, AthleteProfileRepository>();
        services.AddScoped<IProfileLoginRepository, ProfileLoginRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IThemeRepository, ThemeRepository>();
        services.AddScoped<ISiteConfigRepository, SiteConfigRepository>();
        services.AddScoped<ISectionTypeRegistry, SectionTypeRegistry>();
        services.AddScoped<ISanitizationService, SanitizationService>();
        services.AddScoped<IEmailService, LoggingEmailService>();
        services.AddScoped<UserProvisioner>();
        services.AddScoped<InvitationIssuer>();
        services.Configure<InvitationOptions>(_ => { }); // defaults (7-day expiry)

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<NextAtletDbContext>().Database.EnsureCreated();
    }

    /// <summary>Dispatches a request through MediatR in a fresh DI scope (one per "request").</summary>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    /// <summary>Runs a read against the shared in-memory store for assertions.</summary>
    public async Task<T> QueryAsync<T>(Func<NextAtletDbContext, Task<T>> query)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NextAtletDbContext>();
        return await query(context);
    }

    public void Dispose() => _provider.Dispose();
}
