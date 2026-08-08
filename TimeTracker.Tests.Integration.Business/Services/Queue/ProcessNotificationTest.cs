using Autofac;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Notifications.Senders.Tasks.Comments;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
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

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.FirstOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(testContext.ToAddress, actualEmail.EmailTo);
    }
    
    [Fact]
    public async Task ShouldProcessRegistrationNotification()
    {
        var fakeUser = _userFactory.Generate();
        var expectedUser = await _userSeeder.CreatePendingAsync();
        var testContext = new RegistrationNotificationItemContext(
            fakeUser.Email,
            "http://fron.url",
            expectedUser.VerificationToken!
        );

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(testContext.ToAddress, actualEmail.EmailTo);
        Assert.Contains(expectedUser.VerificationToken!, actualEmail.EmailBody);
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
            WorkspaceId = task.Workspace.Id,
            TaskTitle = "Task title",
            UserName = expectedUser.Name
        };

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(testContext.ToAddress, actualEmail.EmailTo);
        Assert.Contains($"/board/{task.Workspace.Id}/task/{task.Id}", actualEmail.EmailBody);
    }

    [Fact]
    public async Task ShouldProcessTaskCommentNotificationWithWorkspaceUrl()
    {
        var task = await _taskSeeder.CreateAsync();
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var testContext = new SetCommentNotificationContext()
        {
            ToAddress = expectedUser.Email,
            Comment = "Test comment",
            TaskId = task.Id,
            WorkspaceId = task.Workspace.Id,
            OwnerName = expectedUser.Name
        };

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);

        var actualEmail = GraylogClient.EmailLogs.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(testContext.ToAddress, actualEmail.EmailTo);
        Assert.Contains($"/board/{task.Workspace.Id}/task/{task.Id}", actualEmail.EmailBody);
    }
}
