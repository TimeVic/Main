using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.NotificationToken;

public class SetTest: BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly IDataFactory<UserNotificationTokenEntity> _notificationTokenFactory;
    private readonly IUserSeeder _userSeeder;

    public SetTest(): base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userNotificationTokenDao = Scope.Resolve<IUserNotificationTokenDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _notificationTokenFactory = Scope.Resolve<IDataFactory<UserNotificationTokenEntity>>();
    }

    [Fact]
    public async Task ShouldSetNew()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var expectedToken = _notificationTokenFactory.Generate();

        var actualToken = await _userNotificationTokenDao.GetByToken(expectedToken.Token);
        Assert.Null(actualToken);
        
        // Act
        await _userNotificationTokenDao.Set(user, expectedToken.Token);
        
        // Assert
        actualToken = await _userNotificationTokenDao.GetByToken(expectedToken.Token);
        Assert.NotNull(actualToken);
        Assert.Equal(expectedToken.Token, actualToken.Token);
        Assert.Equal(user.Id, actualToken.User.Id);
    }
    
    [Fact]
    public async Task ShouldSetForNewUserIfWasSavedForOther()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var user2 = await _userSeeder.CreateActivatedAsync();
        var expectedToken = _notificationTokenFactory.Generate();

        await _userNotificationTokenDao.Set(user2, expectedToken.Token);
        var actualToken = await _userNotificationTokenDao.GetByToken(expectedToken.Token);
        Assert.NotNull(actualToken);
        Assert.Equal(expectedToken.Token, actualToken.Token);
        Assert.Equal(user2.Id, actualToken.User.Id);
        
        // Act
        await _userNotificationTokenDao.Set(user, expectedToken.Token);
        
        // Assert
        actualToken = await _userNotificationTokenDao.GetByToken(expectedToken.Token);
        Assert.NotNull(actualToken);
        Assert.Equal(expectedToken.Token, actualToken.Token);
        Assert.Equal(user.Id, actualToken.User.Id);
    }
}
