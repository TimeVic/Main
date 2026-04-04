using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class FormatTimeTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Theory]
    [InlineData("", "00:00")]
    [InlineData("1", "00:01")]
    [InlineData("10", "00:10")]
    [InlineData("25", "00:25")]
    [InlineData("24", "00:24")]
    [InlineData("1061", "11:01")]
    [InlineData("0060", "01:00")]
    [InlineData("0002", "00:02")]
    [InlineData("123", "01:23")]
    [InlineData("3:26", "03:26")]
    [InlineData("60", "01:00")]
    [InlineData("65", "01:05")]
    [InlineData("99", "01:39")]
    [InlineData("199", "02:39")]
    [InlineData("043", "00:43")]
    public void ShouldFormatTime(string actual, string expect)
    {
        Assert.Equal(expect, _service.FormatTime(actual));
    }
}

