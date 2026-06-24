using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NextAtlet.Application.Common.Errors;

namespace NextAtlet.Api.Filters;

/// <summary>
/// Documents the single <see cref="ApiError"/> failure contract on every action, so Swagger shows the
/// stable <c>errorCode</c> shape clients must handle. Every failure leaves the backend as an
/// <see cref="ApiError"/> — via <see cref="ResultFilter"/> (Result failures) or
/// <see cref="GlobalExceptionHandler"/> (DomainException) — so a blanket 400 response is accurate for
/// all of them. Applied centrally instead of repeating <c>[ProducesResponseType]</c> on each endpoint;
/// an action that declares its own 400 already wins, so this never double-registers.
/// </summary>
public sealed class ApiErrorResponseConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
            foreach (var action in controller.Actions)
            {
                var declaresBadRequest = action.Filters
                    .OfType<ProducesResponseTypeAttribute>()
                    .Any(f => f.StatusCode == StatusCodes.Status400BadRequest);

                if (!declaresBadRequest)
                    action.Filters.Add(
                        new ProducesResponseTypeAttribute(typeof(ApiError), StatusCodes.Status400BadRequest));
            }
    }
}
