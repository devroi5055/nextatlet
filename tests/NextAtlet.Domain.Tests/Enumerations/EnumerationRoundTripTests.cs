using System.Collections;
using System.Reflection;
using FluentAssertions;
using NextAtlet.Domain.Common;

namespace NextAtlet.Domain.Tests.Enumerations;

/// <summary>
/// Reflection-driven invariants for every <see cref="Enumeration"/> subclass in the Domain assembly:
/// each exposes its members via a static <c>All</c>, every member round-trips through <c>FromId</c>,
/// and an unknown id is rejected. One pair of theories guards all current and future enumerations
/// (Billing, Individual, Organization, Membership, Media, Shared, Identity, Verification …).
/// </summary>
public class EnumerationRoundTripTests
{
    public static IEnumerable<object[]> EnumerationTypes =>
        typeof(Enumeration).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Enumeration)))
            .Select(t => new object[] { t });

    [Fact]
    public void Discovers_the_full_set_of_enumerations()
    {
        // Guards against the reflection silently finding nothing (which would make the theories vacuous).
        EnumerationTypes.Count().Should().BeGreaterThanOrEqualTo(20);
    }

    [Theory]
    [MemberData(nameof(EnumerationTypes))]
    public void All_members_round_trip_through_FromId(Type type)
    {
        var all = TryGetAll(type);
        var fromId = type.GetMethod("FromId", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) });
        if (all is null || fromId is null)
            return; // an enumeration that doesn't follow the All/FromId convention — nothing to assert.

        all.Should().NotBeEmpty($"{type.Name}.All should expose its members");

        foreach (var member in all)
        {
            var resolved = (Enumeration)fromId.Invoke(null, new object[] { member.Id })!;

            resolved.Should().Be(member);          // equality is by Id (Enumeration.Equals)
            resolved.Id.Should().Be(member.Id);
            member.Title.Should().NotBeNull();      // touch the required value-object members
            member.ToString().Should().Be(member.Id);
            _ = member.GetHashCode();
        }
    }

    [Theory]
    [MemberData(nameof(EnumerationTypes))]
    public void FromId_with_unknown_id_throws(Type type)
    {
        var fromId = type.GetMethod("FromId", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) });
        if (fromId is null)
            return;

        var act = () => fromId.Invoke(null, new object[] { "__does_not_exist__" });

        // Reflection wraps the thrown ArgumentException in a TargetInvocationException.
        act.Should().Throw<TargetInvocationException>()
           .WithInnerException<ArgumentException>();
    }

    private static IReadOnlyList<Enumeration>? TryGetAll(Type type)
    {
        var prop = type.GetProperty("All", BindingFlags.Public | BindingFlags.Static);
        if (prop?.GetValue(null) is not IEnumerable raw)
            return null;
        return raw.Cast<Enumeration>().ToList();
    }
}
