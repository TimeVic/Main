using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

public class FindByLoginTest : BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;

    public FindByLoginTest() : base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public async Task ShouldFindByLogin()
    {
        var fakeUser = _userFactory.Generate();
        var user = await _userDao.CreatePendingUser(fakeUser.Email);
        var customLogin = "findme_user_" + new Random().Next(1000, 9999);
        await _userDao.ChangeLoginAsync(user, customLogin);
        await FlushDbChanges(isClearSession: true);

        var results = await _userDao.FindByLogin("findme_user");
        Assert.Contains(results, u => u.Id == user.Id);
    }
}
