using Autofac;
using TimeTracker.Business.Notifications.Senders;
using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Notifications.Senders.Tasks.Comments;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Queue;

public class ProcessNotificationTest: BaseTest
{
    private readonly IQueueService _queueService;
    private readonly IUserSeeder _userSeeder;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskCommentDao _taskCommentDao;
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;

    public ProcessNotificationTest(): base()
    {
        _queueService = Scope.Resolve<IQueueService>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _taskCommentDao = Scope.Resolve<ITaskCommentDao>();
        _taskHistoryItemDao = Scope.Resolve<ITaskHistoryItemDao>();
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
        var expectedUser = await _userSeeder.CreatePendingAsync();
        var testContext = new RegistrationNotificationItemContext(expectedUser.Id);

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(expectedUser.Email, actualEmail.EmailTo);
        Assert.Contains(expectedUser.VerificationToken!, actualEmail.EmailBody);
    }
    
    [Fact]
    public async Task ShouldProcessTaskChangedNotification()
    {
        var task = await _taskSeeder.CreateAsync();
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var taskHistoryItem = await _taskHistoryItemDao.Create(task, expectedUser);
        var testContext = new TaskChangedNotificationContext()
        {
            TaskHistoryItemId = taskHistoryItem.Id,
            RecipientUserId = expectedUser.Id
        };

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        var actualEmail = GraylogClient.EmailLogs.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(expectedUser.Email, actualEmail.EmailTo);
        Assert.Contains($"/board/{task.Workspace.Id}/task/{task.Id}", actualEmail.EmailBody);
    }

    [Fact]
    public async Task ShouldProcessTaskCommentNotificationWithWorkspaceUrl()
    {
        var task = await _taskSeeder.CreateAsync();
        var expectedUser = await _userSeeder.CreateActivatedAsync();
        var taskComment = await _taskCommentDao.AddAsync(task, expectedUser, "Test comment");
        var testContext = new SetCommentNotificationContext()
        {
            TaskCommentId = taskComment.Id,
            RecipientUserId = expectedUser.Id
        };

        await _queueService.PushNotificationAsync(testContext);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);

        var actualEmail = GraylogClient.EmailLogs.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(expectedUser.Email, actualEmail.EmailTo);
        Assert.Contains($"/board/{task.Workspace.Id}/task/{task.Id}", actualEmail.EmailBody);
    }
}
