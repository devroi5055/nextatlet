using NextAtlet.Application.Abstractions.Services;
using NextAtlet.Domain.ValueObjects.Sections;

namespace NextAtlet.Infrastructure.Services.SectionRegistry;

/// <summary>
/// Registry of section type validators (Strategy + Registry). Implements the
/// <see cref="ISectionTypeRegistry"/> abstraction the Application layer depends on.
/// New section types are added by registering a validator here.
/// </summary>
public class SectionTypeRegistry : ISectionTypeRegistry
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

    public bool IsSupported(string sectionType)
    {
        return _validators.ContainsKey(sectionType);
    }

    public ValidationResult Validate(SectionData data)
    {
        if (!_validators.TryGetValue(data.TypeKey, out var validator))
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Section type '{data.TypeKey}' is not registered"]
            };
        }

        return validator.Validate(data);
    }
}
