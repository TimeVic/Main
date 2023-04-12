using Autofac;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue;

public class ProcessNotificationTest: BaseTest
{
    private readonly IQueueService _queueService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly ITaskSeeder _taskSeeder;

    public ProcessNotificationTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldProcessNotification()
    {
        var testContext = new TestNotificationItemContext()
        {
            ToAddress = "test@test.com"
        };

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.FirstOrDefault();
        Assert.Contains(testContext.ToAddress, actualEmail.To);
    }
    
    [Fact]
    public async Task ShouldProcessRegistrationNotification()
    {
        var fakeUser = _userFactory.Generate();
        var expectedUser = await _userSeeder.CreatePendingAsync();
        var testContext = new RegistrationNotificationItemContext(
            fakeUser.Email,
            "http://fron.url",
            expectedUser.VerificationToken
        );

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.LastOrDefault();
        Assert.Contains(testContext.ToAddress, actualEmail.To);
        Assert.Contains(expectedUser.VerificationToken, actualEmail.Body);
    }
    
    [Fact]
    public async Task ShouldProcessTaskChangedNotification()
    {
        var task = await _taskSeeder.CreateAsync();
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var testContext = new TaskChangedNotificationContext()
        {
            ToAddress = expectedUser.Email,
            ChangeSet = new Dictionary<string, string?>()
            {
                { "test", "test" }
            },
            TaskId = task.Id,
            TaskTitle = "Task title",
            UserName = expectedUser.Name
        };

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await _queueService.ProcessAsync(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(EmailSendingServiceMock.IsEmailSent);
        var actualEmail = EmailSendingServiceMock.SentMessages.LastOrDefault();
        Assert.Contains(testContext.ToAddress, actualEmail.To);
    }
}
