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

public partial class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/notifications-center/get-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly TaskEntity _task;
    private readonly INotificationCenterService _notificationCenterService;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldIncludeUnreadForReminder()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await CommitDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(_task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        await _notificationCenterService.MarkAllAsRead(_task.User, _workspace);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(FirebaseClientService.SentMessages.Any());
        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(2, actualResponse.TotalCount);
        Assert.Equal(2, actualResponse.Items.Count());
        Assert.All(actualResponse.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.Equal(NotificationActionType.Reminder, item.Type);
            Assert.NotNull(item.Task);
            Assert.NotNull(item.Task.TaskList);
        });
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
        await CommitDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(otherTask);
        await _notificationCenterService.Push(NotificationActionType.Reminder, otherTask.User, otherTask);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(FirebaseClientService.SentMessages.Any());
        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(0, actualResponse.TotalCount);
        Assert.Equal(0, actualResponse.Items.Count());
    }
}
