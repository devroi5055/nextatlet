using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NextAtlet.Infrastructure.Data;

/// <summary>
/// Maps a typed value object to a Postgres <c>jsonb</c> column via System.Text.Json.
/// Honors [JsonPolymorphic]/[JsonDerivedType] (so SectionData round-trips by discriminator)
/// and uses web defaults (camelCase, case-insensitive) to match the API contract.
/// </summary>
public static class JsonbValueConversion
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    // No `where T : class` so nullable VO properties (GlobalSettings?, etc.) bind without CS8634.
    public static PropertyBuilder<T> HasJsonbConversion<T>(this PropertyBuilder<T> builder)
    {
        var converter = new ValueConverter<T, string>(
            value => JsonSerializer.Serialize(value, Options),
            json => JsonSerializer.Deserialize<T>(json, Options)!);

        // Compare/snapshot by serialized form so EF detects in-place edits to the object graph.
        var comparer = new ValueComparer<T>(
            (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
            v => JsonSerializer.Serialize(v, Options).GetHashCode(), // Serialize(null) => "null", safe
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, Options), Options)!);

        builder.HasConversion(converter, comparer).HasColumnType("jsonb");
        return builder;
    }
}
