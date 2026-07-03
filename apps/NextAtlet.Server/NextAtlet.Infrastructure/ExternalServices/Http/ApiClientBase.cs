using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Infrastructure.ExternalServices.Http
{
    /// <summary>
    /// Base for typed API clients. Applies base address, timeout, and auth header
    /// from options so concrete clients don't repeat the wiring.
    /// </summary>
    public abstract class ApiClientBase
    {
        protected HttpClient Http { get; }

        protected ApiClientBase(HttpClient http, ApiClientSettings settings)
        {
            http.BaseAddress = new Uri(settings.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.AccessToken}");
            Http = http;
        }
    }

    public class ApiClientSettings
    {
        public string BaseUrl { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 10;
    }
}
