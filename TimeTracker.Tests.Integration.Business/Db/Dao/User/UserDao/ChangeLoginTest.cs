using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

public class ChangeLoginTest : BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;

    public ChangeLoginTest() : base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public async Task ShouldChangeLogin()
    {
        var fakeUser = _userFactory.Generate();
        var user = await _userDao.CreatePendingUser(fakeUser.Email);
        var newLogin = "custom_login_" + new Random().Next(1000, 9999);

        var updatedUser = await _userDao.ChangeLoginAsync(user, newLogin);
        await FlushDbChanges(isClearSession: true);

        Assert.Equal(newLogin, updatedUser.Login);

        var fetched = await _userDao.GetById(user.Id);
        Assert.Equal(newLogin, fetched!.Login);
    }
}
