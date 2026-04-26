using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class TryParseDurationTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Theory]
    [InlineData("", null)]
    [InlineData("1h", 60)]
    [InlineData("1h 30m", 90)]
    [InlineData("90m", 90)]
    [InlineData("2d", 960)]
    [InlineData("1w", 2400)]
    [InlineData("1d 2h 15m", 615)]
    [InlineData("01:45", 105)]
    [InlineData("1.5h", 90)]
    [InlineData("1,5h", 90)]
    public void ShouldParseSupportedDurationFormats(string input, int? expectedMinutes)
    {
        var isParsed = _service.TryParseDuration(input, out var result);

        Assert.True(isParsed);
        Assert.Equal(
            expectedMinutes == null ? null : TimeSpan.FromMinutes(expectedMinutes.Value),
            result
        );
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1x")]
    [InlineData("1h test")]
    public void ShouldRejectUnsupportedDurationFormats(string input)
    {
        var isParsed = _service.TryParseDuration(input, out var result);

        Assert.False(isParsed);
        Assert.Null(result);
    }
}
