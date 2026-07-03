using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Domain.Common;

public abstract class Enumeration
{
    public required string Id { get; init; }
    public required LocalizedText Title { get; init; }
    public LocalizedText? Description { get; init; }

    public override string ToString() => Id;
    public override bool Equals(object? obj) => obj is Enumeration other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Enumeration? left, Enumeration? right) => Equals(left, right);
    public static bool operator !=(Enumeration? left, Enumeration? right) => !Equals(left, right);
}
