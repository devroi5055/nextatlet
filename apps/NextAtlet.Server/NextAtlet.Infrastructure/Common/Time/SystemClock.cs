using NextAtlet.Application.Common.Time;

namespace NextAtlet.Infrastructure.Common.Time
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
