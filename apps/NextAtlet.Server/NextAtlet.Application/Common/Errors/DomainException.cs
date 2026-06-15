namespace NextAtlet.Application.Common.Errors;

/// <summary>
/// An expected, user-facing failure. Carries a stable <see cref="ErrorCode"/> (never a localized
/// string) plus structured <see cref="Parameters"/>; the frontend resolves the code to text in the
/// active locale. System/infrastructure failures must NOT use this — they stay plain exceptions,
/// are logged, and surface as a generic 500.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }
    public IReadOnlyList<object> Parameters { get; }

    public DomainException(string errorCode, params object[] parameters)
        : base(errorCode) // base message = code, for logs only
    {
        ErrorCode = errorCode;
        Parameters = parameters ?? [];
    }
}
