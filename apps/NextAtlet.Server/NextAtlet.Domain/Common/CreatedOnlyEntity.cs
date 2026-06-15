namespace NextAtlet.Domain.Common
{
    public abstract class CreatedOnlyEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedUtc { get; private set; }
    }
}