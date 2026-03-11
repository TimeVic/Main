using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue.Handlers;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Notification.NotificationCenterService.Task;

public partial class TaskNotificationHandlerTest: BaseTest
{
    [Fact]
    public async System.Threading.Tasks.Task ShouldHandleReminder()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        _task.RemindedTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_task);

        // Act
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        
        // Assert
        Assert.True(FirebaseClientService.SentMessages.Any());

        var actualListDto = await _notificationCenterService.GetList(_user, _task.Workspace);
        Assert.Equal(1, actualListDto.TotalCount);
        Assert.Contains(actualListDto.Items, item =>
        {
            return item.Workspace.Id == _task.Workspace.Id
                && item.Task != null
                && item.Task.Id == _task.Id
                && item.ReceiverUser.Id == _task.User.Id;
        });
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldHandleViaQueue()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        _task.RemindedTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(_task);

        // Act
        await _queueService.PushDefaultAsync(new NotificationCenterPushRequestContext()
        {
            Action = NotificationActionType.Reminder,
            TaskId = _task.Id,
            ProducedUserId = _task.User.Id
        });
        await _queueService.ProcessAsync(QueueChannel.Default);
        
        // Assert
        Assert.True(FirebaseClientService.SentMessages.Any());

        var actualListDto = await _notificationCenterService.GetList(_user, _task.Workspace);
        Assert.Equal(1, actualListDto.TotalCount);
        Assert.Contains(actualListDto.Items, item =>
        {
            return item.Workspace.Id == _task.Workspace.Id
                && item.Task != null
                && item.Task.Id == _task.Id
                && item.ReceiverUser.Id == _task.User.Id;
        });
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
    }
}
