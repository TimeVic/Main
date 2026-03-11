using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.NotificationsCenter;

public partial class MarkAllAsReadTest: BaseTest
{
    private readonly string Url = "/dashboard/notifications-center/mark-all-as-read";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly TaskEntity _task;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public MarkAllAsReadTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _userNotificationTokenDao = ServiceProvider.GetRequiredService<IUserNotificationTokenDao>();
        _notificationCenterService = ServiceProvider.GetRequiredService<INotificationCenterService>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _task = _taskSeeder.CreateAsync(user: _user).Result;
        DbSessionProvider.PerformCommitAsync().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new MarkAllAsReadRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldMarkAllAsRead()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(_task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);

        await FlushDbChanges();
        Assert.Equal(2, await _notificationCenterService.GetUnreadCount(_user, _workspace));
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new MarkAllAsReadRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _workspace));
    }
    
    [Fact]
    public async Task ShouldMarkForOtherUser()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(_task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        
        var otherUser = await UserSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, otherUser, MembershipAccessType.User);
        var otherTask = await _taskSeeder.CreateAsync(user: otherUser);
        
        await _userNotificationTokenDao.Set(otherUser, FirebaseClientServiceMock.SuccessToken);
        otherTask.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(otherTask);
        await _notificationCenterService.Push(NotificationActionType.Reminder, otherTask.User, otherTask);
        await FlushDbChanges();
        
        Assert.Equal(2, await _notificationCenterService.GetUnreadCount(_user, _workspace));
        await DbSessionProvider.CurrentSession.RefreshAsync(otherTask.Workspace);
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(otherUser, otherTask.Workspace));
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new MarkAllAsReadRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(otherUser, otherTask.Workspace));
    }
}
