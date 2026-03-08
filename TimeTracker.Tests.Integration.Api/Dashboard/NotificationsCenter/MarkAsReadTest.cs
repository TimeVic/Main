using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.NotificationsCenter;

public partial class MarkAsReadTest: BaseTest
{
    private readonly string Url = "/dashboard/notifications-center/mark-as-read";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly TaskEntity _task;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public MarkAsReadTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _userNotificationTokenDao = ServiceProvider.GetRequiredService<IUserNotificationTokenDao>();
        _notificationCenterService = ServiceProvider.GetRequiredService<INotificationCenterService>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _task = _taskSeeder.CreateAsync(user: _user).Result;
        _task.ReminderTime = DateTime.UtcNow;
        DbSessionProvider.PerformCommitAsync().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        await _notificationCenterService.Push(NotificationActionType.Reminder, _user, _task);
        var notifications = await _notificationCenterService.GetList(_user, _task.Workspace);
        var notification = notifications.Items.First();
        
        var response = await PostRequestAsAnonymousAsync(Url, new MarkAsReadRequest()
        {
            NotificationId = notification.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldMarkAsRead()
    {
        // Arrange
        await _notificationCenterService.Push(NotificationActionType.Reminder, _user, _task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _user, _task);
        var notifications = await _notificationCenterService.GetList(_user, _task.Workspace);
        var notification = notifications.Items.First();
        
        Assert.Equal(2, await _notificationCenterService.GetUnreadCount(_user, _workspace));
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new MarkAsReadRequest()
        {
            NotificationId = notification.Id
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(_user, _workspace));
    }
    
    [Fact]
    public async Task ShouldNotMarkForOtherUser()
    {
        // Arrange
        var otherUser = await UserSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, otherUser, MembershipAccessType.User);
        var otherTask = await _taskSeeder.CreateAsync(user: otherUser);
        
        await _userNotificationTokenDao.Set(otherUser, FirebaseClientServiceMock.SuccessToken);
        otherTask.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(otherTask);
        await _notificationCenterService.Push(NotificationActionType.Reminder, otherTask.User, otherTask);
        await _notificationCenterService.Push(NotificationActionType.Reminder, otherTask.User, otherTask);
        await FlushDbChanges();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(otherTask);
        Assert.Equal(2, await _notificationCenterService.GetUnreadCount(otherUser, otherTask.Workspace));
        
        var notifications = await _notificationCenterService.GetList(otherUser, otherTask.Workspace);
        var notification = notifications.Items.First();
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new MarkAsReadRequest()
        {
            NotificationId = notification.Id
        });
        
        // Assert
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.Type);
    }
}
