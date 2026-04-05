using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class ParseTimeSpanTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Fact]
    public void ShouldParseTime()
    {
        Assert.Equal(TimeSpan.Parse("0:01"), _service.ParseTimeSpan("1"));
        Assert.Equal(TimeSpan.Parse("0:10"), _service.ParseTimeSpan("10"));
        Assert.Equal(TimeSpan.Parse("0:25"), _service.ParseTimeSpan("25"));
        Assert.Equal(TimeSpan.Parse("11:01"), _service.ParseTimeSpan("1061"));
        Assert.Equal(TimeSpan.Parse("1:00"), _service.ParseTimeSpan("0060"));
    }
}

