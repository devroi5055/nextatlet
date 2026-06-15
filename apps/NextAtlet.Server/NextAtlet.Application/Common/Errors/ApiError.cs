namespace NextAtlet.Application.Common.Errors;

/// <summary>
/// The single error response shape — this IS the API error contract (see docs/01).
/// The backend emits a stable <paramref name="ErrorCode"/> + structured parameters; the frontend
/// resolves the code to a localized message and interpolates the parameters.
/// </summary>
public record ApiError(string ErrorCode, IReadOnlyList<object> Parameters);
