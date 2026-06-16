namespace NextAtlet.Domain.Common
{
    public interface IRetirable
    {
        public DateTime? RetiredUtc { get; }
        bool IsRetired { get; }
        void Retire(DateTime utcNow);
    }
}