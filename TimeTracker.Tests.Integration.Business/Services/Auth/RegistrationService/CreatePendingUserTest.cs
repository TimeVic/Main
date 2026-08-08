using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
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
    private readonly IUserDao _userDao;
    private new readonly IQueueDao _queueDao;

    public CreatePendingUserTest(): base()
    {
        _authService = Scope.Resolve<IRegistrationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _queueService = Scope.Resolve<IQueueService>();
        _userDao = Scope.Resolve<IUserDao>();
        _queueDao = Scope.Resolve<IQueueDao>();

        _queueDao.CompleteAllPending();
    }

    [Fact]
    public async Task ShouldCreateAndSendNotification()
    {
        var expectedEmail = _userFactory.Generate().Email;
        
        var user = await _authService.CreatePendingUser(expectedEmail);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.False(user.IsActivated);
        Assert.Equal(expectedEmail.ToLower(), user.Email);
        Assert.Equal("en", user.Language.Code);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.FirstOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(user.Email, actualEmail.EmailTo);
    }

    [Fact]
    public async Task ShouldCreateWithRequestedLanguage()
    {
        var expectedEmail = _userFactory.Generate().Email;

        var user = await _authService.CreatePendingUser(expectedEmail, "uk-UA");

        Assert.Equal("uk-UA", user.Language.Code);
    }

    [Fact]
    public async Task ShouldNotCreateDuplicateDefaultWorkspaceWhenResendingNotification()
    {
        var expectedEmail = _userFactory.Generate().Email;

        var user = await _authService.CreatePendingUser(expectedEmail);
        await FlushDbChanges();

        await _authService.CreatePendingUser(expectedEmail);
        await FlushDbChanges();

        var workspaces = await _userDao.GetUsersWorkspaces(user, MembershipAccessType.Owner);
        Assert.Single(workspaces, item => item.IsDefault);
    }
    
    [Fact]
    public async Task ShouldReSendNotificationIfExists()
    {
        var expectedEmail = _userFactory.Generate().Email;
        
        var user = await _authService.CreatePendingUser(expectedEmail);
        await QueueProcess(QueueChannel.Notifications);
        GraylogClient.Clear();

        await _authService.CreatePendingUser(expectedEmail);
        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.FirstOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(user.Email, actualEmail.EmailTo);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfEmailExists()
    {
        var expectedEmail = _userFactory.Generate().Email;
        
        var actualUser = await _authService.CreatePendingUser(expectedEmail);
        await FlushDbChanges();
        await _authService.ActivateUser(actualUser.VerificationToken!, "some password");

        await Assert.ThrowsAsync<RecordIsExistsException>(async () =>
        {
            await _authService.CreatePendingUser(expectedEmail);
        });
    }
}
