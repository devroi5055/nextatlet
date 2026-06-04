using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NextAtlet.Api;
using NextAtlet.Application;
using NextAtlet.Application.Abstractions.Identity;
using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Infrastructure.Data;
using NextAtlet.Infrastructure.Persistence;
using NextAtlet.Infrastructure.Persistence.Repositories;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<OAuthSecuritySchemeTransformer>());

// OAuth2 / OIDC bearer authentication (Auth0). Tokens are validated against the configured issuer.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier // map Auth0 'sub' to NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Prefer the httpOnly access_token cookie (XSS-safe transport); fall back to the
                // Authorization header so tooling/machine clients (and Swagger) still work.
                if (context.Request.Cookies.TryGetValue("access_token", out var cookieToken)
                    && !string.IsNullOrEmpty(cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            }
        };
    });

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
builder.Services.AddScoped<IAthleteProfileRepository, AthleteProfileRepository>();
builder.Services.AddScoped<IProfileLoginRepository, ProfileLoginRepository>();
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();
builder.Services.AddScoped<ISiteConfigRepository, SiteConfigRepository>();

// Domain services (behind Application abstractions)
builder.Services.AddScoped<ISectionTypeRegistry, SectionTypeRegistry>();
builder.Services.AddScoped<ISanitizationService, SanitizationService>();

// Current user resolved from the validated token's claims (all environments).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

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
        dbContext.Database.Migrate();
    }
}

app.Run();
