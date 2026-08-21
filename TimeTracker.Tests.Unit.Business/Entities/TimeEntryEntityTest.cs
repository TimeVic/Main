using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Tests.Unit.Business.Entities;

public class TimeEntryEntityTest
{
    [Fact]
    public void IsSyncedShouldReturnFalseWhenNoExternalIdIsSet()
    {
        var entity = new TimeEntryEntity();

        Assert.False(entity.IsSynced);
    }

    [Fact]
    public void IsSyncedShouldReturnTrueWhenRedmineIdIsSet()
    {
        var entity = new TimeEntryEntity
        {
            RedmineId = "12345"
        };

        Assert.True(entity.IsSynced);
    }

    [Fact]
    public void IsSyncedShouldReturnTrueWhenClickUpIdIsSet()
    {
        var entity = new TimeEntryEntity
        {
            ClickUpId = "cu_123"
        };

        Assert.True(entity.IsSynced);
    }

    [Fact]
    public void IsSyncedShouldReturnTrueWhenJiraIdIsSet()
    {
        var entity = new TimeEntryEntity
        {
            JiraId = 10001
        };

        Assert.True(entity.IsSynced);
    }
}
