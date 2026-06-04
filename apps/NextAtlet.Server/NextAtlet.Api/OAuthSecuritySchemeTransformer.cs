using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace NextAtlet.Api;

/// <summary>
/// Adds an OAuth2 (Authorization Code + PKCE) security scheme to the generated OpenAPI document so
/// Swagger UI shows an "Authorize" button and sends the bearer token. Endpoints are matched by the
/// global security requirement; the Auth0 endpoints are derived from the configured Authority.
/// </summary>
public sealed class OAuthSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IConfiguration _configuration;

    public OAuthSecuritySchemeTransformer(IConfiguration configuration) => _configuration = configuration;

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authority = (_configuration["Authentication:Authority"] ?? string.Empty).TrimEnd('/');
        var scopes = (_configuration["Authentication:Swagger:Scopes"] ?? "openid profile email")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToDictionary(scope => scope, _ => string.Empty);

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/authorize"),
                    TokenUrl = new Uri($"{authority}/oauth/token"),
                    Scopes = scopes
                }
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["oauth2"] = scheme;

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("oauth2", document)] = scopes.Keys.ToList()
        });

        return Task.CompletedTask;
    }
}
