using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NextAtlet.Application.Common.Errors;
using DomainResult = NextAtlet.Application.Common.Results.IResult;

namespace NextAtlet.Api.Filters;

/// <summary>
/// Unwraps a <see cref="NextAtlet.Application.Common.Results.Result{T}"/> returned by a controller so the
/// Result envelope stays internal to the backend: success → the bare value (200), or 204 when there is
/// nothing to return (empty success); failure → an <see cref="ApiError"/> body with a 400. Controllers just
/// <c>return Ok(result)</c>; clients never see the wrapper.
///
/// The failure body is the <see cref="ApiError"/> contract — a stable, frontend-mappable <c>errorCode</c>
/// (not the internal dev-facing <c>Error.Message</c>). User-facing failures flow through here as Result
/// errors; unexpected exceptions are handled separately by <see cref="GlobalExceptionHandler"/> (generic
/// 500). The frontend resolves the code to a localized message.
/// </summary>
public sealed class ResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: DomainResult result })
        {
            context.Result = result.IsSuccess
                ? Success(result.Value)
                : new ObjectResult(ToApiError(result.Error!)) { StatusCode = StatusCodes.Status400BadRequest };
        }

        await next();
    }

    // Project the internal Error onto the public ApiError contract: transport the stable code, drop the
    // dev-only message. Result-based failures carry no structured parameters, so the list is empty.
    private static ApiError ToApiError(NextAtlet.Application.Common.Results.Error error) =>
        new(error.Code, []);

    // Nothing meaningful to return (null payload, or a Unit-valued result) is an empty success.
    private static IActionResult Success(object? value) =>
        value is null or Unit
            ? new StatusCodeResult(StatusCodes.Status204NoContent)
            : new ObjectResult(value) { StatusCode = StatusCodes.Status200OK };
}
