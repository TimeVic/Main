using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

public class GenerateUniqueLoginTest : BaseTest
{
    private readonly IUserDao _userDao;

    public GenerateUniqueLoginTest() : base()
    {
        _userDao = Scope.Resolve<IUserDao>();
    }

    [Fact]
    public async Task ShouldGenerateUniqueLoginFromEmail()
    {
        var email = "john.doe+testing@example.com";
        var login = await _userDao.GenerateUniqueLogin(email);
        Assert.Equal("john_doe_testing", login);
    }

    [Fact]
    public async Task ShouldHandleShortEmailWhenGeneratingLogin()
    {
        var email = "a@example.com";
        var login = await _userDao.GenerateUniqueLogin(email);
        Assert.True(login.Length >= 3);
        Assert.StartsWith("a", login);
    }

    [Fact]
    public async Task ShouldGenerateNumberedLoginOnCollision()
    {
        var email1 = "testuser@example.com";
        var email2 = "testuser@otherdomain.com";

        var user1 = await _userDao.CreatePendingUser(email1);
        await FlushDbChanges();
        Assert.Equal("testuser", user1.Login);

        var login2 = await _userDao.GenerateUniqueLogin(email2);
        Assert.Equal("testuser_1", login2);
    }
}
