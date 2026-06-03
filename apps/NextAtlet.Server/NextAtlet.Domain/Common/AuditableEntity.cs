namespace NextAtlet.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedUtc { get; private set; }
        public DateTime UpdatedUtc { get; private set; }

        public void SetCreated()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = CreatedUtc;
        }

        public void SetUpdated()
        {
            UpdatedUtc = DateTime.UtcNow;
        }
    }
}