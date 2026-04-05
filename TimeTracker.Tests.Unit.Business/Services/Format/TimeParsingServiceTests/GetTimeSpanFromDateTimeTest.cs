using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Services.Format.TimeParsingServiceTests;

public class GetTimeSpanFromDateTimeTest
{
    private readonly ITimeParsingService _service = new TimeParsingService();

    [Fact]
    public void ShouldReceiveTimeSpanFromDateTime()
    {
        Assert.Equal(
            TimeSpan.Parse("1:00"),
            _service.GetTimeSpanFromDateTime(DateTime.Now.StartOfDay().AddHours(1))
        );

        Assert.Equal(
            TimeSpan.Parse("23:00"),
            _service.GetTimeSpanFromDateTime(DateTime.Now.StartOfDay().AddHours(-1))
        );

        Assert.Equal(
            TimeSpan.Parse("1:00"),
            _service.GetTimeSpanFromDateTime(DateTime.Now.AddDays(-1).StartOfDay().AddHours(1))
        );
    }
}

