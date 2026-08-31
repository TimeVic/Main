using Autofac;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.User.UserDao;

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
        var expectedUser = _userFactory.Generate();

        var actualUser = await _userDao.CreatePendingUser(expectedUser.Email);
        Assert.True(actualUser.Id != Guid.Empty);
        Assert.Equal(expectedUser.Email.ToLower(), actualUser.Email);
        Assert.False(string.IsNullOrEmpty(actualUser.Login));
        Assert.NotNull(actualUser.VerificationToken);
        Assert.Null(actualUser.VerificationTime);
        Assert.Equal(TimeZoneInfo.Utc.Id, actualUser.Timezone);
        Assert.False(actualUser.IsActivated);
        Assert.Empty(actualUser.PasswordHash);
        Assert.Empty(actualUser.PasswordSalt);
        Assert.True(actualUser.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.True(actualUser.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.NotNull(actualUser.Language);
        Assert.Equal("en", actualUser.Language.Code);
    }
}
