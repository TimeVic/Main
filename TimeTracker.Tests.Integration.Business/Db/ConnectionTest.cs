using Autofac;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db;

public class ConnectionTest: BaseTest
{
    private IDataFactory<UserEntity> _userFactory;
    private ILanguageDao _languageDao;

    public ConnectionTest(): base()
    {
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _languageDao = Scope.Resolve<ILanguageDao>();
    }

    [Fact]
    public void TestDbConnection()
    {
        FlushDbChanges().Wait();
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
        user.Language = await _languageDao.GetDefaultAsync();
        var expectedTime = DateTime.UtcNow;
        user.CreatedAt = expectedTime;
        user.UpdatedAt = expectedTime;
        
        var userId = await DbSessionProvider.CurrentSession.SaveAsync(user);
        await FlushDbChanges();

        user = await DbSessionProvider.CurrentSession.GetAsync<UserEntity>(userId);
        Assert.Equal(expectedTime.Date, user.CreatedAt.Date);
        Assert.Equal(expectedTime.Hour, user.CreatedAt.Hour);
        Assert.Equal(expectedTime.Minute, user.CreatedAt.Minute);
        Assert.Equal(expectedTime.Second, user.CreatedAt.Second);
        Assert.Equal(expectedTime.Millisecond, user.CreatedAt.Millisecond);
    }
}
