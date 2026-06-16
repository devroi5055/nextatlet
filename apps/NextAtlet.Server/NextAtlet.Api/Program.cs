using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NextAtlet.Api;
using NextAtlet.Api.Filters;
using NextAtlet.Application;
using NextAtlet.Application.Common.Options;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.Account;
using NextAtlet.Application.Features.Invitations;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.Authorization;
using NextAtlet.Infrastructure.Common.Time;
using NextAtlet.Infrastructure.Data;
using NextAtlet.Infrastructure.Persistence;
using NextAtlet.Infrastructure.Persistence.Repositories;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;
using System.Net.Http.Headers;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<ResultFilter>());
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<OAuthSecuritySchemeTransformer>());

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
builder.Services.AddScoped<IAthleteProfileRepository, AthleteProfileRepository>();
builder.Services.AddScoped<ISiteLoginRepository, SiteLoginRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IGuardianConsentRepository, GuardianConsentRepository>();
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();
builder.Services.AddScoped<ISiteSnapshotRepository, SiteSnapshotRepository>();

// Domain services (behind Application abstractions)
builder.Services.AddScoped<ISectionTypeRegistry, SectionTypeRegistry>();
builder.Services.AddScoped<ISanitizationService, SanitizationService>();
builder.Services.AddSingleton<IClock, SystemClock>();

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

// Application services shared across handlers (identity provisioning + invitation issuing)
builder.Services.AddScoped<UserProvisioner>();
builder.Services.AddScoped<InvitationIssuer>();
builder.Services.AddSingleton<PermissionResolver>(); // stateless: ControlMode + role → permissions
builder.Services.Configure<InvitationOptions>(builder.Configuration.GetSection(InvitationOptions.SectionName));
builder.Services.Configure<AgeThresholdOptions>(builder.Configuration.GetSection(AgeThresholdOptions.SectionName));
// Handlers inject AgeThresholdOptions directly (not IOptions<>), so expose the resolved value.
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AgeThresholdOptions>>().Value);
builder.Services.Configure<TermsOptions>(builder.Configuration.GetSection(TermsOptions.SectionName));


// Add CORS (for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous(); // serves the OpenAPI document at /openapi/v1.json (Swagger must read it unauthenticated)
    app.UseSwaggerUI(options =>
    {
        // Swagger UI (at /swagger) reads the document produced by AddOpenApi() — for manual testing.
        options.SwaggerEndpoint("/openapi/v1.json", "NextAtlet API v1");

        // OAuth2 login from Swagger ("Authorize" button).
        options.OAuthClientId(builder.Configuration["Authentication:Swagger:ClientId"]);
        options.OAuthUsePkce();
        options.OAuthScopeSeparator(" ");
        // Auth0 only issues a JWT access token for the API when the 'audience' param is present.
        var audience = builder.Configuration["Authentication:Audience"];
        if (!string.IsNullOrWhiteSpace(audience))
            options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { ["audience"] = audience });
    });
    app.UseCors("Development");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply EF Core migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NextAtletDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }
}

app.Run();
