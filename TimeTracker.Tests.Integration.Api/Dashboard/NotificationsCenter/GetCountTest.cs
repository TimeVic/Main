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

public partial class GetCountTest: BaseTest
{
    private readonly string Url = "/dashboard/notifications-center/get-count";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly TaskEntity _task;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetCountTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _userNotificationTokenDao = ServiceProvider.GetRequiredService<IUserNotificationTokenDao>();
        _notificationCenterService = ServiceProvider.GetRequiredService<INotificationCenterService>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetCountRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldIncludeOnlyUnreadForReminder()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await FlushDbChanges();

        await FlushAndRefreshEntity(_task);
        await FlushAndRefreshEntity(_workspace);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        await FlushDbChanges();
        await _notificationCenterService.MarkAllAsRead(_task.User, _workspace);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetCountRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(FirebaseClientService.SentMessages.Any());
        var actualResponse = await response.GetJsonDataAsync<GetCountResponse>();
        Assert.Equal(1, actualResponse.UnreadCount);
    }
    
    [Fact]
    public async Task ShouldNotIncludeIfOtherUsersHasNotifications()
    {
        // Arrange
        var otherUser = await UserSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(_workspace, otherUser, MembershipAccessType.User);
        var otherTask = await _taskSeeder.CreateAsync(user: otherUser);
        
        await _userNotificationTokenDao.Set(otherUser, FirebaseClientServiceMock.SuccessToken);
        otherTask.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await _notificationCenterService.Push(NotificationActionType.Reminder, otherTask.User, otherTask);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetCountRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        await response.EnsureSuccessStatusCodeWithoutError();
        Assert.True(FirebaseClientService.SentMessages.Any());
        var actualResponse = await response.GetJsonDataAsync<GetCountResponse>();
        Assert.Equal(0, actualResponse.UnreadCount);
    }
}
