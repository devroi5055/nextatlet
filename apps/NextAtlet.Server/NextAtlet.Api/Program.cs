using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NextAtlet.Api;
using NextAtlet.Api.Filters;
using NextAtlet.Api.Seeding;
using NextAtlet.Application;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.ActionTokens.Strategies;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Authorization;
using NextAtlet.Infrastructure.Common.Time;
using NextAtlet.Infrastructure.ExternalServices.Cvr;
using NextAtlet.Infrastructure.ExternalServices.Scrape;
using NextAtlet.Infrastructure.Persistence;
using NextAtlet.Infrastructure.Persistence.Repositories;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;
using System.Net.Http.Headers;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Controllers + their cross-cutting filters (ResultFilter, default error responses) are registered
// together further down under "Http code filters".
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<DefaultApiErrorResponseFilter>();

    var authority = builder.Configuration["Authentication:Authority"];

    // Only add the security scheme if Authority is configured — guards against
    // new Uri(null...) throwing during spec generation.
    if (!string.IsNullOrWhiteSpace(authority))
    {
        var baseUri = authority.EndsWith("/") ? authority : authority + "/";   // ensure trailing slash

        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{baseUri}authorize"),
                    TokenUrl = new Uri($"{baseUri}oauth/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        ["openid"] = "OpenID",
                        ["profile"] = "Profile",
                        ["email"] = "Email"
                    }
                }
            }
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("oauth2", document)] = []
        });
    }
});
// Dual-scheme auth: cookie (production / Next.js session) + JWT bearer (Swagger, service clients).
// A "smart" policy scheme routes each request to the right handler so [Authorize] endpoints serve
// both transparently. Claim extraction (ClaimsPrincipalExtensions) is scheme-agnostic.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "smart";
    options.DefaultChallengeScheme = "smart";
})
.AddCookie("cookie", options =>
{
    options.Cookie.Name = "nextatlet.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // This is an API, not an MVC app — there's no login page to redirect to.
    // Return 401/403 instead of redirecting, so unauthenticated requests don't
    // loop endlessly to a nonexistent /Account/Login.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
})
.AddJwtBearer("bearer", options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.Audience = builder.Configuration["Authentication:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier // map Auth0 'sub' to NameIdentifier
    };
})
.AddPolicyScheme("smart", "smart", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        return authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? "bearer"   // Swagger / machine clients
            : "cookie";  // Next.js frontend session
    };
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultFilter>();
});

// Auth0 access tokens omit email by default — point this at the namespaced claim an Action adds.
ClaimsPrincipalExtensions.ConfiguredEmailClaimType = builder.Configuration["Authentication:EmailClaimType"];

// Authenticated by default: every endpoint requires a valid token unless it opts out with
// [AllowAnonymous]. New endpoints are locked unless deliberately opened.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// ProblemDetails + global exception handling (replaces per-action try/catch)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=nextatlet;Username=postgres;Password=postgres";

builder.Services.AddDbContext<NextAtletDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

// CQRS via MediatR — handlers live in the Application assembly
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IApplicationMarker).Assembly));

// Repositories + Unit of Work (EF implementations over the shared scoped DbContext)
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISiteRepository, SiteRepository>();
builder.Services.AddScoped<IIndividualProfileRepository, IndividualProfileRepository>();
builder.Services.AddScoped<IOrganizationProfileRepository, OrganizationProfileRepository>();
builder.Services.AddScoped<ISiteLoginRepository, SiteLoginRepository>();
builder.Services.AddScoped<IActionTokenRepository, ActionTokenRepository>();
builder.Services.AddScoped<IGuardianConsentRepository, GuardianConsentRepository>();
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();
builder.Services.AddScoped<ISiteSnapshotRepository, SiteSnapshotRepository>();

// Domain services (behind Application abstractions)
builder.Services.AddScoped<ISectionTypeRegistry, SectionTypeRegistry>();
builder.Services.AddScoped<ISanitizationService, SanitizationService>();
builder.Services.AddSingleton<IClock, SystemClock>();

//ActionToken // TODO: might change to singelton
builder.Services.AddScoped<ActionTokenStrategyRegistry>();
builder.Services.AddScoped<IActionTokenStrategy, OrgEmailVerificationStrategy>();
builder.Services.AddScoped<IActionTokenStrategy, ConsentStrategy>();
builder.Services.AddScoped<IActionTokenStrategy, InvitationStrategy>();

builder.Services.AddCvrLookup(builder.Configuration);

// Email: send real invite mail via Resend when an API key is configured; otherwise log the link
// (so local dev needs no secrets). Either way handlers depend only on IEmailService.
var emailSection = builder.Configuration.GetRequiredSection(EmailOptions.SectionName);
builder.Services.Configure<EmailOptions>(emailSection);
if (!string.IsNullOrWhiteSpace(emailSection[nameof(EmailOptions.InviteApiKey)]))
{
    builder.Services.AddHttpClient<IEmailService, ResendEmailService>((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
        client.BaseAddress = new Uri("https://api.resend.com/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.InviteApiKey);
    });
}
else
{
    builder.Services.AddScoped<IEmailService, LoggingEmailService>();
}

// Application services shared across handlers (identity provisioning)
builder.Services.AddScoped<UserProvisioner>();
builder.Services.AddSingleton<PermissionResolver>(); // stateless: ControlMode + role → permissions
builder.Services.Configure<InvitationOptions>(builder.Configuration.GetSection(InvitationOptions.SectionName));
builder.Services.Configure<AgeThresholdOptions>(builder.Configuration.GetSection(AgeThresholdOptions.SectionName));
// Handlers inject AgeThresholdOptions directly (not IOptions<>), so expose the resolved value.
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AgeThresholdOptions>>().Value);
builder.Services.Configure<TermsOptions>(builder.Configuration.GetSection(TermsOptions.SectionName));

//strategies
//builder.Services.AddScoped<IVerificationStrategy, CvrVerificationStrategy>();

//club import: scraper strategies + canonicalizer + repository
builder.Services.AddScoped<IClubSourceStrategy, DjuPortalScraper>();
builder.Services.AddScoped<IClubCanonicalizer, ClubCanonicalizer>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();

// Add CORS (for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policyBuilder =>
    {
        // Reflect the request origin (any localhost port in dev) rather than
        // "*", because the SPA sends credentialed requests (cookies) and
        // browsers reject a wildcard ACAO when credentials are included.
        policyBuilder.SetIsOriginAllowed(_ => true)
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NextAtlet API v1");

        options.OAuthClientId(builder.Configuration["Authentication:Swagger:ClientId"]);
        options.OAuthUsePkce();
        options.OAuthScopeSeparator(" ");

        var audience = builder.Configuration["Authentication:Audience"];
        if (!string.IsNullOrWhiteSpace(audience))
        {
            options.OAuthAdditionalQueryStringParams(
                new Dictionary<string, string> { ["audience"] = audience }
            );
        }
    });

    app.UseCors("Development");
}

// Don't force HTTPS in development: the SPA calls the http endpoint (5278) and
// a redirect to https (7162) would break the CORS preflight and hit the
// self-signed dev cert.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply EF Core migrations on startup (development only), then seed a small sample dataset.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NextAtletDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }

    await DevelopmentDataSeeder.SeedAsync(app.Services);
}
app.Run();