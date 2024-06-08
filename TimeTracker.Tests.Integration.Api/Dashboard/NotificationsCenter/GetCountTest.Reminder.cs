using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.NotificationsCenter;

public partial class GetCountTest: BaseTest
{
    [Fact]
    public async Task ShouldGetForReminder()
    {
        // Arrange
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        _task.ReminderTime = DateTime.UtcNow.Add(GlobalConstants.TaskReminderTimeout).AddMinutes(-1);
        await CommitDbChanges();

        await DbSessionProvider.CurrentSession.RefreshAsync(_task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
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
        Assert.Equal(2, actualResponse.UnreadCount);
    }
}
