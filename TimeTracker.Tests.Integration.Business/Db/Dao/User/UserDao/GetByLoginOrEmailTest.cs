using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

public class GetByLoginOrEmailTest : BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;

    public GetByLoginOrEmailTest() : base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public async Task ShouldGetByLoginOrEmail()
    {
        var fakeUser = _userFactory.Generate();
        var user = await _userDao.CreatePendingUser(fakeUser.Email);
        await FlushDbChanges(isClearSession: true);

        var fetchedByLoginOrEmailViaLogin = await _userDao.GetByLoginOrEmail(user.Login!);
        Assert.NotNull(fetchedByLoginOrEmailViaLogin);
        Assert.Equal(user.Id, fetchedByLoginOrEmailViaLogin.Id);

        var fetchedByLoginOrEmailViaEmail = await _userDao.GetByLoginOrEmail(user.Email);
        Assert.NotNull(fetchedByLoginOrEmailViaEmail);
        Assert.Equal(user.Id, fetchedByLoginOrEmailViaEmail.Id);
    }
}
