using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class TimeSpanToTimeStringTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Theory]
    [InlineData(1, 30, 0, false, "1:30")]
    [InlineData(0, 5, 0, false, "0:05")]
    [InlineData(10, 0, 0, false, "10:00")]
    [InlineData(1, 30, 45, true, "1:30:45")]
    [InlineData(0, 0, 9, true, "0:00:09")]
    [InlineData(23, 59, 59, true, "23:59:59")]
    public void ShouldFormatTimeSpan(int hours, int minutes, int seconds, bool addSecond, string expected)
    {
        var timeSpan = new TimeSpan(hours, minutes, seconds);
        Assert.Equal(expected, _service.TimeSpanToTimeString(timeSpan, addSecond));
    }

    [Fact]
    public void ShouldNotIncludeSecondsWhenFlagIsFalse()
    {
        var result = _service.TimeSpanToTimeString(new TimeSpan(1, 2, 3));
        Assert.Equal("1:02", result);
    }

    [Fact]
    public void ShouldParseTimeSpanToString()
    {
        Assert.Equal(
            "1:00",
            _service.TimeSpanToTimeString(TimeSpan.FromHours(1))
        );

        var timeSpanFromTime = DateTime.Parse("2020-04-30T03:00:00.000Z").ToUniversalTime().AddDays(-2).TimeOfDay;
        Assert.Equal(
            "3:00",
            _service.TimeSpanToTimeString(timeSpanFromTime)
        );

        timeSpanFromTime = DateTime.Parse("2020-04-30T12:11:10.000Z").ToUniversalTime().TimeOfDay;
        Assert.Equal(
            "12:11:10",
            _service.TimeSpanToTimeString(timeSpanFromTime, true)
        );

        timeSpanFromTime = DateTime.Parse("2020-04-30T23:59:59.999Z").ToUniversalTime().AddDays(-2).TimeOfDay;
        Assert.Equal(
            "23:59",
            _service.TimeSpanToTimeString(timeSpanFromTime)
        );
    }
}


