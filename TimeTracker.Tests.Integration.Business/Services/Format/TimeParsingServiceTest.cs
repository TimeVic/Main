using Autofac;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Format;

public class TimeParsingServiceTest: BaseTest
{
    private readonly ITimeParsingService _timeParsingService;

    public TimeParsingServiceTest(): base()
    {
        _timeParsingService = Scope.Resolve<ITimeParsingService>();
    }

    [Theory]
    [InlineData("", "0:00")]
    [InlineData("1", "1:00")]
    [InlineData("10", "10:00")]
    [InlineData("25", "0:00")]
    [InlineData("24", "0:00")]
    [InlineData("1061", "11:00")]
    [InlineData("0060", "1:00")]
    [InlineData("0002", "0:02")]
    [InlineData("123", "12:30")]
    public void ShouldFormatTime(string actual, string expect)
    {
        Assert.Equal(expect, _timeParsingService.FormatTime(actual));
    }
    
    [Fact]
    public void ShouldParseTime()
    {
        Assert.Equal(DateTimeOffset.Parse("1:00"), _timeParsingService.ParseTime("1"));
        Assert.Equal(DateTimeOffset.Parse("10:00"), _timeParsingService.ParseTime("10"));
        Assert.Equal(DateTimeOffset.Parse("0:00"), _timeParsingService.ParseTime("25"));
        Assert.Equal(DateTimeOffset.Parse("11:00"), _timeParsingService.ParseTime("1061"));
        Assert.Equal(DateTimeOffset.Parse("1:00"), _timeParsingService.ParseTime("0060"));
    }
}
