using Microsoft.AspNetCore.Diagnostics;
using NextAtlet.Application.Common.Errors;

namespace NextAtlet.Api;

/// <summary>
/// The single place that turns exceptions into responses (Model A — see docs/01, docs/07).
/// User-facing <see cref="DomainException"/> → 400 + its error code; everything else is logged
/// and returned as a generic 500 that leaks no internal detail.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ApiError body;

        switch (exception)
        {
            case DomainException domain:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                body = new ApiError(domain.ErrorCode, domain.Parameters);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                body = new ApiError(ErrorCodes.Internal, []);
                break;
        }

        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);
        return true;
    }
}
