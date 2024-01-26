using Autofac;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Notification;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Notification.Push;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Notification.NotificationCenterService.Task;

public partial class TaskCommentNotificationHandlerTest: BaseTest
{
    private readonly IFileStorage _fileStorage;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserSeeder _userSeeder;
    
    private readonly TaskEntity _task;
    private readonly UserEntity _user;
    private readonly IStoredFilesDao _storedFilesDao;
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly ITaskNotificationService _notificationBgJobService;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly ITaskCommentSeeder _taskCommentSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IQueueService _queueService;

    public TaskCommentNotificationHandlerTest(): base()
    {
        _fileStorage = Scope.Resolve<IFileStorage>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _storedFilesDao = Scope.Resolve<IStoredFilesDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _userNotificationTokenDao = Scope.Resolve<IUserNotificationTokenDao>();
        _notificationBgJobService = Scope.Resolve<ITaskNotificationService>();
        _gcmNotificationService = Scope.Resolve<IGcmNotificationService>();
        _notificationCenterService = Scope.Resolve<INotificationCenterService>();
        _taskCommentSeeder = Scope.Resolve<ITaskCommentSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _queueService = Scope.Resolve<IQueueService>();
        _user = _userSeeder.CreateActivatedAsync().Result;

        _storedFilesDao.MarkAsUploadedAllPending().Wait();
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldHandleIfCommentAdded()
    {
        // Arrange
        var watcher1 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_task.Workspace, watcher1, MembershipAccessType.User);
        
        var comment = await _taskCommentSeeder.CreateAsync(_task, _user, new List<UserEntity>()
        {
            watcher1
        });
        
        await _userNotificationTokenDao.Set(watcher1, FirebaseClientServiceMock.SuccessToken2);
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        await CommitDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(comment);
        
        // Act
        await _notificationCenterService.Push(NotificationActionType.AddEntity, _user, comment);
        
        // Assert
        Assert.True(FirebaseClientService.SentMessages.Any());

        var actualListDto = await _notificationCenterService.GetList(watcher1, _task.Workspace);
        Assert.Equal(1, actualListDto.TotalCount);
        Assert.Contains(actualListDto.Items, item =>
        {
            return item.Workspace.Id == _task.Workspace.Id
                && item.Type == NotificationActionType.AddEntity
                && item.TaskComment != null
                && item.TaskComment.Id == comment.Id
                && item.ReceiverUser.Id == watcher1.Id;
        });
        // Should not send to produced user
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(watcher1, _task.Workspace));
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldHandleIfCommentUpdated()
    {
        // Arrange
        var producedUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_task.Workspace, producedUser, MembershipAccessType.User);
        
        var watcher1 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_task.Workspace, watcher1, MembershipAccessType.User);
        
        var comment = await _taskCommentSeeder.CreateAsync(_task, _user, new List<UserEntity>()
        {
            watcher1
        });
        
        await _userNotificationTokenDao.Set(watcher1, FirebaseClientServiceMock.SuccessToken2);
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        await CommitDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(comment);
        
        // Act
        await _notificationCenterService.Push(NotificationActionType.EditEntity, producedUser, comment);
        
        // Assert
        Assert.True(FirebaseClientService.SentMessages.Any());

        var actualListDto = await _notificationCenterService.GetList(watcher1, _task.Workspace);
        Assert.Equal(1, actualListDto.TotalCount);
        Assert.Contains(actualListDto.Items, item =>
        {
            return item.Workspace.Id == _task.Workspace.Id
                && item.Type == NotificationActionType.EditEntity
                && item.TaskComment != null
                && item.TaskComment.Id == comment.Id
                && item.ReceiverUser.Id == watcher1.Id;
        });
        // Should not send to produced user
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(producedUser, _task.Workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(watcher1, _task.Workspace));
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldHandleViaQueue()
    {
        // Arrange
        var watcher1 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_task.Workspace, watcher1, MembershipAccessType.User);
        
        var comment = await _taskCommentSeeder.CreateAsync(_task, _user, new List<UserEntity>()
        {
            watcher1
        });
        
        await _userNotificationTokenDao.Set(watcher1, FirebaseClientServiceMock.SuccessToken2);
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        
        await CommitDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(comment);
        
        // Act
        await _queueService.PushDefaultAsync(new NotificationCenterPushRequestContext()
        {
            Action = NotificationActionType.AddEntity,
            TaskCommentId = comment.Id,
            ProducedUserId = _user.Id
        });
        await _queueService.ProcessAsync(QueueChannel.Default);
        
        // Assert
        Assert.True(FirebaseClientService.SentMessages.Any());

        var actualListDto = await _notificationCenterService.GetList(watcher1, _task.Workspace);
        Assert.Equal(1, actualListDto.TotalCount);
        Assert.Contains(actualListDto.Items, item =>
        {
            return item.Workspace.Id == _task.Workspace.Id
                && item.Type == NotificationActionType.AddEntity
                && item.TaskComment != null
                && item.TaskComment.Id == comment.Id
                && item.ReceiverUser.Id == watcher1.Id;
        });
        // Should not send to produced user
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(watcher1, _task.Workspace));
    }
}
