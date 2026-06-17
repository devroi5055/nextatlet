using Microsoft.Extensions.Options;
using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Interfaces.Services;
using NextAtlet.Infrastructure.ExternalServices.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace NextAtlet.Infrastructure.ExternalServices.Cvr
{
    public class CvrHttpService : ApiClientBase, ICvrLookupService
    {
        public CvrHttpService(HttpClient http, IOptions<CvrApiOptions> options)
            : base(http, new ApiClientSettings
            {
                BaseUrl = options.Value.BaseUrl,
                AccessToken = options.Value.AccessToken,
                TimeoutSeconds = options.Value.TimeoutSeconds
            })
        { }

        public async Task<JsonElement?> LookupAsync(string cvrNumber, CancellationToken ct)
        {
            
            var response = await Http.GetAsync($"api/v2/dk/company/{cvrNumber}", ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<JsonElement>(content)
                : null;
        }
    }
}
