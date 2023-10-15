using Autofac;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Auth.ResetPassword;

public class ChangePasswordTest: BaseTest
{
    private readonly IRegistrationService _registrationService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IQueueService _queueService;
    private readonly IQueueDao _queueDao;
    private readonly IUserDao _userDao;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly IPasswordService _passwordService;

    public ChangePasswordTest(): base()
    {
        _registrationService = Scope.Resolve<IRegistrationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userDao = Scope.Resolve<IUserDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _queueService = Scope.Resolve<IQueueService>();
        _resetPasswordService = Scope.Resolve<IResetPasswordService>();
        _passwordService = Scope.Resolve<IPasswordService>();
        _queueDao = Scope.Resolve<IQueueDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _queueDao.CompleteAllPending();
    }

    [Fact]
    public async Task ShouldChangePassword()
    {
        var newPassword = "Some123NewPass";
        var newRequest = await _resetPasswordService.Generate(_user);
        await _resetPasswordService.ChangePassword(newRequest.VerificationToken, newPassword);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_user);
        await DbSessionProvider.CurrentSession.RefreshAsync(newRequest);
        
        Assert.True(newRequest.IsExpired);
        Assert.True(_passwordService.ValidatePassword(_user, newPassword));
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfExpired()
    {
        var previousRequest = await _resetPasswordService.Generate(_user);
        previousRequest.ExpirationTime = DateTime.UtcNow.AddMinutes(-1);
        await CommitDbChanges();
        await Assert.ThrowsAsync<RecordExpiredException>(async () =>
        {
            await _resetPasswordService.ChangePassword(previousRequest.VerificationToken, "Some123NewPass");
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfNotFound()
    {
        await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
        {
            await _resetPasswordService.ChangePassword("asdasd", "Some123NewPass");
        });
    }
}
