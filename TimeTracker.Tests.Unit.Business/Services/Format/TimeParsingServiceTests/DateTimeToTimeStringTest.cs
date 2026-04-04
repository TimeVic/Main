using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class DateTimeToTimeStringTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Theory]
    [InlineData(1, 30, 0, false, "1:30")]
    [InlineData(0, 5, 0, false, "0:05")]
    [InlineData(10, 0, 0, false, "10:00")]
    [InlineData(1, 30, 45, true, "1:30:45")]
    [InlineData(0, 0, 9, true, "0:00:09")]
    [InlineData(23, 59, 59, true, "23:59:59")]
    public void ShouldFormatDateTime(int hours, int minutes, int seconds, bool addSecond, string expected)
    {
        var dateTime = new DateTime(2026, 4, 4, hours, minutes, seconds, DateTimeKind.Utc);
        Assert.Equal(expected, _service.DateTimeToTimeString(dateTime, addSecond));
    }

    [Fact]
    public void ShouldNotIncludeSecondsWhenFlagIsFalse()
    {
        var dateTime = new DateTime(2026, 4, 4, 1, 2, 3, DateTimeKind.Utc);
        Assert.Equal("1:02", _service.DateTimeToTimeString(dateTime));
    }

    [Fact]
    public void ShouldIgnoreDatePart()
    {
        var dt1 = new DateTime(2024, 1, 1, 8, 15, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(2026, 12, 31, 8, 15, 0, DateTimeKind.Utc);
        Assert.Equal(_service.DateTimeToTimeString(dt1), _service.DateTimeToTimeString(dt2));
    }

    [Fact]
    public void ShouldReturnEmptyStringForNull()
    {
        Assert.Equal(string.Empty, _service.DateTimeToTimeString((DateTime?)null));
    }

    [Fact]
    public void ShouldFormatNullableDateTime()
    {
        DateTime? dateTime = new DateTime(2026, 4, 4, 9, 5, 0, DateTimeKind.Utc);
        Assert.Equal("9:05", _service.DateTimeToTimeString(dateTime));
    }
}

