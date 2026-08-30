using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

public class IsLoginExistsTest : BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;

    public IsLoginExistsTest() : base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public async Task ShouldCheckLoginExists()
    {
        var fakeUser = _userFactory.Generate();
        var user = await _userDao.CreatePendingUser(fakeUser.Email);
        await FlushDbChanges(isClearSession: true);

        Assert.True(await _userDao.IsLoginExistsAsync(user.Login!));
        Assert.False(await _userDao.IsLoginExistsAsync(user.Login!, user.Id));
        Assert.False(await _userDao.IsLoginExistsAsync("non_existent_login_" + new Random().Next(10000, 99999)));
    }
}
