using NextAtlet.Application.Common.DTOs;
using System.Text.Json;

namespace NextAtlet.Application.Abstractions.Services;

/// <summary>
/// Looks up a Danish organization by CVR number against the official register.
/// Implemented in Infrastructure as a typed HttpClient; consumed by the CVR
/// verification strategy. Returns null when the CVR is not found.
/// </summary>
public interface ICvrLookupService
{
    Task<JsonElement?> LookupAsync(string cvrNumber, CancellationToken ct);
}
