using Autofac;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
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
    [InlineData("1", "0:01")]
    [InlineData("10", "0:10")]
    [InlineData("25", "0:25")]
    [InlineData("24", "0:24")]
    [InlineData("1061", "11:01")]
    [InlineData("0060", "1:00")]
    [InlineData("0002", "0:02")]
    [InlineData("123", "1:23")]
    [InlineData("3:26", "3:26")]
    [InlineData("60", "1:00")]
    [InlineData("65", "1:05")]
    [InlineData("99", "1:39")]
    [InlineData("199", "2:39")]
    [InlineData("043", "0:43")]
    public void ShouldFormatTime(string actual, string expect)
    {
        Assert.Equal(expect, _timeParsingService.FormatTime(actual));
    }
    
    [Fact]
    public void ShouldParseTime()
    {
        Assert.Equal(TimeSpan.Parse("0:01"), _timeParsingService.ParseTime("1"));
        Assert.Equal(TimeSpan.Parse("0:10"), _timeParsingService.ParseTime("10"));
        Assert.Equal(TimeSpan.Parse("0:25"), _timeParsingService.ParseTime("25"));
        Assert.Equal(TimeSpan.Parse("11:01"), _timeParsingService.ParseTime("1061"));
        Assert.Equal(TimeSpan.Parse("1:00"), _timeParsingService.ParseTime("0060"));
    }
    
    [Fact]
    public void ShouldReceiveTimeSpanFromDateTime()
    {
        Assert.Equal(
            TimeSpan.Parse("1:00"), 
            _timeParsingService.GetTimeSpanFromDateTime(DateTime.Now.StartOfDay().AddHours(1))
        );
        
        Assert.Equal(
            TimeSpan.Parse("23:00"), 
            _timeParsingService.GetTimeSpanFromDateTime(DateTime.Now.StartOfDay().AddHours(-1))
        );
        
        Assert.Equal(
            TimeSpan.Parse("1:00"), 
            _timeParsingService.GetTimeSpanFromDateTime(DateTime.Now.AddDays(-1).StartOfDay().AddHours(1))
        );
    }
    
    [Fact]
    public void ShouldParseTimeSpanToString()
    {
        Assert.Equal(
            "1:00", 
            _timeParsingService.TimeSpanToTimeString(TimeSpan.FromHours(1))
        );

        var timeSpanFromTime = DateTime.Parse("2020-04-30T03:00:00.000Z").ToUniversalTime().AddDays(-2).TimeOfDay;
        Assert.Equal(
            "3:00", 
            _timeParsingService.TimeSpanToTimeString(timeSpanFromTime)
        );
        
        timeSpanFromTime = DateTime.Parse("2020-04-30T12:11:10.000Z").ToUniversalTime().TimeOfDay;
        Assert.Equal(
            "12:11:10", 
            _timeParsingService.TimeSpanToTimeString(timeSpanFromTime, true)
        );
    }
}
