namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Registry of section type validators.
/// Ensures the validator seam exists early, allowing new types to be added without retrofitting.
/// </summary>
public class SectionTypeRegistry
{
    private readonly Dictionary<string, ISectionValidator> _validators = new();

    public SectionTypeRegistry()
    {
        // Register step 1 validators
        Register(new HeroSectionValidator());
        Register(new BioSectionValidator());
    }

    public void Register(ISectionValidator validator)
    {
        _validators[validator.SectionType] = validator;
    }

    public ISectionValidator? GetValidator(string sectionType)
    {
        return _validators.TryGetValue(sectionType, out var validator) ? validator : null;
    }

    public IEnumerable<string> GetSupportedTypes()
    {
        return _validators.Keys;
    }

    public bool IsSupported(string sectionType)
    {
        return _validators.ContainsKey(sectionType);
    }
}
