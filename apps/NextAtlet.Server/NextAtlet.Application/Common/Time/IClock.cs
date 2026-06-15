namespace NextAtlet.Application.Common.Time
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
