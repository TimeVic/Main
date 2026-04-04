using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Extensions.DateTimeTests;

public class WithDateTest
{
    [Fact]
    public void ShouldKeepTimeAndKindForDateTime()
    {
        var source = new DateTime(2026, 4, 4, 14, 30, 15, DateTimeKind.Utc);
        var newDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Local);

        var actual = source.WithDate(newDate);

        Assert.Equal(new DateTime(2026, 5, 1, 14, 30, 15, DateTimeKind.Utc), actual);
    }

    [Fact]
    public void ShouldReturnNullForNullableDateTimeWhenSourceIsNull()
    {
        DateTime? source = null;

        var actual = source.WithDate(new DateTime(2026, 5, 1));

        Assert.Null(actual);
    }

    [Fact]
    public void ShouldKeepTimeAndKindForNullableDateTime()
    {
        DateTime? source = new DateTime(2026, 4, 4, 6, 45, 0, DateTimeKind.Local);

        var actual = source.WithDate(new DateTime(2026, 5, 1));

        Assert.Equal(new DateTime(2026, 5, 1, 6, 45, 0, DateTimeKind.Local), actual);
    }
}

