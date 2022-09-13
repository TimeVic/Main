using Autofac;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Auth.RegistrationService;

public class CreatePendingUserTest: BaseTest
{
    private readonly IRegistrationService _authService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IQueueService _queueService;
    private readonly IQueueDao _queueDao;

    public CreatePendingUserTest(): base()
    {
        _authService = Scope.Resolve<IRegistrationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _queueService = Scope.Resolve<IQueueService>();
        _queueDao = Scope.Resolve<IQueueDao>();

        _queueDao.CompleteAllPending();
    }

    [Fact]
    public async Task ShouldCreateAndSendNotification()
    {
        var expectedEmail = _userFactory.Generate().Email;
        
        var user = await _authService.CreatePendingUser(expectedEmail);
        Assert.True(user.Id > 0);
        Assert.False(user.IsActivated);
        Assert.Equal(expectedEmail.ToLower(), user.Email);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingService.IsEmailSent);
        var actualEmail = EmailSendingService.SentMessages.FirstOrDefault();
        Assert.Contains(user.Email, actualEmail.To);
    }
    
    [Fact]
    public async Task ShouldReSendNotificationIfExists()
    {
        var expectedEmail = _userFactory.Generate().Email;
        
        var user = await _authService.CreatePendingUser(expectedEmail);
        await _queueService.ProcessAsync(QueueChannel.Notifications);
        EmailSendingService.Reset();

        await _authService.CreatePendingUser(expectedEmail);
        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingService.IsEmailSent);
        var actualEmail = EmailSendingService.SentMessages.FirstOrDefault();
        Assert.Contains(user.Email, actualEmail.To);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfEmailExists()
    {
        var expectedEmail = _userFactory.Generate().Email;
        
        var actualUser = await _authService.CreatePendingUser(expectedEmail);
        await _authService.ActivateUser(actualUser.VerificationToken, "some password");

        await Assert.ThrowsAsync<RecordIsExistsException>(async () =>
        {
            await _authService.CreatePendingUser(expectedEmail);
        });
    }
}
