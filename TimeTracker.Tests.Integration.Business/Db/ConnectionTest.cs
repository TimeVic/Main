using Autofac;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db;

public class ConnectionTest: BaseTest
{
    private IDataFactory<UserEntity> _userFactory;

    public ConnectionTest(): base()
    {
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public void TestDbConnection()
    {
        DbSessionProvider.PerformCommitAsync().Wait();
    }
    
    [Fact]
    public async Task TimeZoneShouldBeUtc()
    {
        var result = await DbSessionProvider.CurrentSession.CreateSQLQuery("show timezone;").ListAsync<string>();
        Assert.Equal("Etc/UTC", result.First());
    }
    
    [Fact]
    public async Task ShouldInsertRecordsWithDateTimeInUtcTimezone()
    {
        var user = _userFactory.Generate();
        user.PasswordHash = Array.Empty<byte>();
        user.PasswordSalt = Array.Empty<byte>();
        var expectedTime = DateTime.UtcNow;
        user.CreateTime = expectedTime;
        user.UpdateTime = expectedTime;
        
        var userId = await DbSessionProvider.CurrentSession.SaveAsync(user);
        await CommitDbChanges();

        user = await DbSessionProvider.CurrentSession.GetAsync<UserEntity>(userId);
        Assert.Equal(expectedTime.Date, user.CreateTime.Date);
        Assert.Equal(expectedTime.Hour, user.CreateTime.Hour);
        Assert.Equal(expectedTime.Minute, user.CreateTime.Minute);
        Assert.Equal(expectedTime.Second, user.CreateTime.Second);
        Assert.Equal(expectedTime.Millisecond, user.CreateTime.Millisecond);
    }
}
