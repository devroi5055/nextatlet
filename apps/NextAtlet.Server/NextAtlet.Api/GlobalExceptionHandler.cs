using Microsoft.AspNetCore.Diagnostics;
using NextAtlet.Application.Common.Errors;

namespace NextAtlet.Api;

/// <summary>
/// The single place that turns unhandled exceptions into responses (Model A — see docs/01, docs/07).
/// User-facing failures travel as <c>Result</c> errors (unwrapped to a 400 + error code by
/// <see cref="Filters.ResultFilter"/>), so anything that surfaces here is an unexpected/system failure:
/// it's logged and returned as a generic 500 that leaks no internal detail.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ApiError(ErrorCodes.Internal, []), cancellationToken);
        return true;
    }
}
