using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Abstractions.Services;

namespace NextAtlet.Infrastructure.ExternalServices.Cvr;

public static class ServiceCollection
{
    public static IServiceCollection AddCvrLookup(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<CvrApiOptions>()
            .Bind(config.GetSection(CvrApiOptions.SectionName))
            .ValidateOnStart();

        // no configure lambda — the client configures itself from injected options
        services.AddHttpClient<ICvrLookupService, CvrHttpService>();

        return services;
    }
}