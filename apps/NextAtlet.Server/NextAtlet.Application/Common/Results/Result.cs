namespace NextAtlet.Application.Common.Results;

/// <summary>
/// A failure. <see cref="Code"/> is a stable, frontend-mappable key (see <c>ErrorCodes</c>) — never a
/// localized string; the frontend resolves it to text. <see cref="Message"/> is a plain developer-facing
/// fallback, not for end-user display.
/// </summary>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Build an error from a catalog code (see <c>ErrorCodes</c>) — the durable, transport-agnostic
    /// contract. The code doubles as the dev-facing message; the frontend localizes by code.
    /// </summary>
    public static Error FromCode(string code) => new(code, code);
}

/// <summary>
/// Non-generic view of <see cref="Result{T}"/> so the API result filter can unwrap any result without
/// knowing its payload type. Internal to the backend — never serialized.
/// </summary>
public interface IResult
{
    bool IsSuccess { get; }
    object? Value { get; }
    Error? Error { get; }
}

public class Result<T> : IResult
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(Error e) { IsSuccess = false; Error = e; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error e) => new(e);
    public static Result<T> FailureFromCode(string code) => new(Error.FromCode(code));

    // ergonomics: lets you `return dto;` or `return new Error(...);`
    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error e) => new(e);

    object? IResult.Value => Value;
}
