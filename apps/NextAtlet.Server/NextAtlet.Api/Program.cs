using MediatR;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddOpenApi();

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

// Current user resolved from the validated token's claims
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
    app.MapOpenApi();
    app.UseCors("Development");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Auto-migrate on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NextAtletDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.Run();
