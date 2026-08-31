using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

public class GetByLoginTest : BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;

    public GetByLoginTest() : base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public async Task ShouldGetByLogin()
    {
        var fakeUser = _userFactory.Generate();
        var user = await _userDao.CreatePendingUser(fakeUser.Email);
        await FlushDbChanges(isClearSession: true);

        var fetchedByLogin = await _userDao.GetByLogin(user.Login!);
        Assert.NotNull(fetchedByLogin);
        Assert.Equal(user.Id, fetchedByLogin.Id);

        var fetchedByLoginWithAt = await _userDao.GetByLogin("@" + user.Login!);
        Assert.NotNull(fetchedByLoginWithAt);
        Assert.Equal(user.Id, fetchedByLoginWithAt.Id);
    }
}
