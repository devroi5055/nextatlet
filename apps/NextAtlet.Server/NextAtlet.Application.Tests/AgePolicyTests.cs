using NextAtlet.Domain.Policies;
using Xunit;

namespace NextAtlet.Application.Tests;

public class AgePolicyTests
{
    [Theory]
    [InlineData("2000-06-15", "2018-06-14", 17)] // day before 18th birthday
    [InlineData("2000-06-15", "2018-06-15", 18)] // exactly on 18th birthday
    [InlineData("2000-06-15", "2018-06-16", 18)] // day after
    [InlineData("2004-02-29", "2022-02-28", 17)] // leap-year birthday, non-leap year: not yet "29th"
    [InlineData("2004-02-29", "2022-03-01", 18)] // leap-year birthday, day after
    public void AgeAt_counts_completed_years(string dob, string on, int expected)
    {
        var age = AgePolicy.AgeAt(DateOnly.Parse(dob), DateOnly.Parse(on));
        Assert.Equal(expected, age);
    }

    [Fact]
    public void BandToday_classifies_exact_boundaries()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        Assert.Equal(AgeBand.BelowMinimum, AgePolicy.BandToday(today.AddYears(-12)));        // 12
        Assert.Equal(AgeBand.BelowMinimum, AgePolicy.BandToday(today.AddYears(-13).AddDays(1))); // 13th birthday tomorrow → still 12
        Assert.Equal(AgeBand.YoungMinor,   AgePolicy.BandToday(today.AddYears(-13)));        // exactly 13
        Assert.Equal(AgeBand.YoungMinor,   AgePolicy.BandToday(today.AddYears(-15)));        // 15
        Assert.Equal(AgeBand.OlderMinor,   AgePolicy.BandToday(today.AddYears(-16)));        // exactly 16
        Assert.Equal(AgeBand.OlderMinor,   AgePolicy.BandToday(today.AddYears(-17)));        // 17
        Assert.Equal(AgeBand.Adult,        AgePolicy.BandToday(today.AddYears(-18)));        // exactly 18
        Assert.Equal(AgeBand.Adult,        AgePolicy.BandToday(today.AddYears(-40)));        // 40
    }
}
