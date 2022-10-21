using Autofac;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Localization;

public class LocalizationListTest: BaseTest
{
    public LocalizationListTest(): base()
    {
    }

    [Fact]
    public void ShouldReceiveTimeZones()
    {
        var timezones = TimeZoneInfo.GetSystemTimeZones();
        Assert.Contains(timezones, item => item.Id == "UTC");
    }
    
    [Fact]
    public void ShouldReceiveUtcTimeZone()
    {
        var utcTimeZone = TimeZoneInfo.Utc.Id;
        Assert.Equal("UTC", utcTimeZone);
    }
}
