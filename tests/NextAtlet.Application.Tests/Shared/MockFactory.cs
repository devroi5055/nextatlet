using NextAtlet.Application.Common.Time;
using NSubstitute;

public static class MockFactory
{
    public static IClock CreateClock(DateTime utcNow)
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(utcNow);
        return clock;
    }
}