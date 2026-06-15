using AutoFixture;

namespace NextAtlet.Application.Tests.Shared.TestData;

/// <summary>
/// A configured AutoFixture <see cref="Fixture"/> shared by the entity builders — same setup the
/// handler fixtures use (the <see cref="SectionDataSpecimentBuilder"/> for the abstract SectionData,
/// a concrete DateOnly, and recursion omission for navigation graphs).
/// </summary>
internal static class TestFixture
{
    public static Fixture Create()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new SectionDataSpecimentBuilder());
        fixture.Register<DateOnly>(() => new DateOnly(Random.Shared.Next(1990, 2010), 1, 1));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
