using System.Reflection;
using System.Text.Json;
using NextAtlet.Application.Common.Errors;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// Locks the code↔catalog contract: every ErrorCodes constant must have a matching translation in
/// BOTH da.json and en.json. Catches drift at build time so no user ever sees a raw error key.
/// The catalogs here are the canonical source the frontend mirrors.
/// </summary>
public class ErrorCatalogTests
{
    [Fact]
    public void Every_error_code_has_da_and_en_translations()
    {
        var codes = typeof(ErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        Assert.NotEmpty(codes);

        var da = LoadCatalogKeys("da.json");
        var en = LoadCatalogKeys("en.json");

        foreach (var code in codes)
        {
            Assert.True(da.Contains(code), $"Missing 'da' translation for error code '{code}'");
            Assert.True(en.Contains(code), $"Missing 'en' translation for error code '{code}'");
        }
    }

    private static HashSet<string> LoadCatalogKeys(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ErrorCatalogs", fileName);
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
    }
}
