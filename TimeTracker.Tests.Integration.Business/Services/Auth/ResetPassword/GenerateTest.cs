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

public class GenerateTest: BaseTest
{
    private readonly IRegistrationService _registrationService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IQueueService _queueService;
    private readonly IQueueDao _queueDao;
    private readonly IUserDao _userDao;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;
    private readonly IResetPasswordService _resetPasswordService;

    public GenerateTest(): base()
    {
        _registrationService = Scope.Resolve<IRegistrationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userDao = Scope.Resolve<IUserDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _queueService = Scope.Resolve<IQueueService>();
        _resetPasswordService = Scope.Resolve<IResetPasswordService>();
        _queueDao = Scope.Resolve<IQueueDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _queueDao.CompleteAllPending();
    }

    [Fact]
    public async Task ShouldGenerateNew()
    {
        var newRequest = await _resetPasswordService.Generate(_user);
        
        Assert.NotNull(newRequest);
        Assert.Equal(_user.Id, newRequest.User.Id);
        Assert.NotEmpty(newRequest.VerificationToken);
        Assert.True(newRequest.ExpirationTime > DateTime.UtcNow);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfPreviousWasNotExpired()
    {
        await _resetPasswordService.Generate(_user);
        await Assert.ThrowsAsync<TooManyRequestsException>(async () =>
        {
            await _resetPasswordService.Generate(_user);
        });
    }
    
    [Fact]
    public async Task ShouldGenerateNewIfPreviousExpired()
    {
        var previousRequest = await _resetPasswordService.Generate(_user);
        previousRequest.ExpirationTime = DateTime.UtcNow.AddMinutes(-1);
        await CommitDbChanges();
        
        var actualRequest = await _resetPasswordService.Generate(_user);
        Assert.NotEqual(actualRequest.Id, previousRequest.Id);
    }
    
    [Fact]
    public async Task ShouldSendNotificationAfterGeneration()
    {
        var newRequest = await _resetPasswordService.Generate(_user);
        
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(SmtpClientServiceMock.IsEmailSent);
        var actualEmail = SmtpClientServiceMock.SentMessages.FirstOrDefault();
        Assert.Contains(_user.Email, actualEmail.To);
        Assert.Contains(newRequest.VerificationToken, actualEmail.Body);
    }
}
