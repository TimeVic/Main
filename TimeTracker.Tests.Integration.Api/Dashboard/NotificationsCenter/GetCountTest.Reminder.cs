using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
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
        
        await FlushAndRefreshEntity(_task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        await _notificationCenterService.Push(NotificationActionType.Reminder, _task.User, _task);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new GetCountRequest()
        {
            WorkspaceId = _workspace.Id
        });
        
        // Assert
        await response.EnsureSuccessStatusCodeWithoutError();
        Assert.True(FirebaseClientService.SentMessages.Any());
        var actualResponse = await response.GetJsonDataAsync<GetCountResponse>();
        Assert.Equal(2, actualResponse.UnreadCount);
    }
}
