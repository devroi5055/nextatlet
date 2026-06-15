using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DomainResult = NextAtlet.Application.Common.Results.IResult;

namespace NextAtlet.Api.Filters;

/// <summary>
/// Unwraps a <see cref="NextAtlet.Application.Common.Results.Result{T}"/> returned by a controller so the
/// Result envelope stays internal to the backend: success → the bare value (200), or 204 when there is
/// nothing to return (empty success); failure → the <c>Error</c> body with a 400. Controllers just
/// <c>return Ok(result)</c>; clients never see the wrapper.
/// </summary>
public sealed class ResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: DomainResult result })
        {
            context.Result = result.IsSuccess
                ? Success(result.Value)
                : new ObjectResult(result.Error) { StatusCode = StatusCodes.Status400BadRequest };
        }

        await next();
    }

    // Nothing meaningful to return (null payload, or a Unit-valued result) is an empty success.
    private static IActionResult Success(object? value) =>
        value is null or Unit
            ? new StatusCodeResult(StatusCodes.Status204NoContent)
            : new ObjectResult(value) { StatusCode = StatusCodes.Status200OK };
}
