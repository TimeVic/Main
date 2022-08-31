using Autofac;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User;

public class CreatePendingUserTest: BaseTest
{
    private readonly IUserDao _userDao;
    private readonly IDataFactory<UserEntity> _userFactory;

    public CreatePendingUserTest(): base()
    {
        _userDao = Scope.Resolve<IUserDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
    }

    [Fact]
    public async Task ShouldCreate()
    {
        var fakeUser = _userFactory.Generate();
        
        var user = await _userDao.CreatePendingUser(fakeUser.Email);
        Assert.True(user.Id > 0);
        Assert.NotEmpty(user.VerificationToken);
        Assert.NotEmpty(user.Email);
        Assert.Null(user.VerificationTime);
    }
}
