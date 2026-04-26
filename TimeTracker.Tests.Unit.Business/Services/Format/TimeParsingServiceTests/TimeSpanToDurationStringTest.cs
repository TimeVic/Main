using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class TimeSpanToDurationStringTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Theory]
    [InlineData(null, "")]
    [InlineData(0, "0m")]
    [InlineData(45, "45m")]
    [InlineData(60, "1h")]
    [InlineData(90, "1h 30m")]
    [InlineData(480, "1d")]
    [InlineData(615, "1d 2h 15m")]
    [InlineData(2400, "1w")]
    [InlineData(2910, "1w 1d 30m")]
    public void ShouldFormatDurationString(int? totalMinutes, string expected)
    {
        TimeSpan? timeSpan = totalMinutes == null ? null : TimeSpan.FromMinutes(totalMinutes.Value);

        Assert.Equal(expected, _service.TimeSpanToDurationString(timeSpan));
    }
}
