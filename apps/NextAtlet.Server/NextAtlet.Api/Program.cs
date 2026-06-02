using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Features.Athletes.Commands;
using NextAtlet.Application.Features.Athletes.Queries;
using NextAtlet.Infrastructure.Data;
using NextAtlet.Infrastructure.Services;
using NextAtlet.Infrastructure.Services.SectionRegistry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// Register application services
builder.Services.AddScoped<SectionTypeRegistry>();
builder.Services.AddScoped<SanitizationService>();
builder.Services.AddScoped<CreateAthleteCommand>();
builder.Services.AddScoped<GetDraftConfigQuery>();
builder.Services.AddScoped<UpdateDraftConfigCommand>();

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
