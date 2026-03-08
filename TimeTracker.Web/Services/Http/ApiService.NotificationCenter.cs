using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<int> NotificationCenterGetUnreadCount(Guid workspaceId)
        {
            var response = await PostAsync<GetCountResponse>(ApiUrl.NotificationCenterGetCount, new GetCountRequest()
            {
                WorkspaceId = workspaceId
            });
            if (response == null)
            {
                return 0;
            }

            return response.UnreadCount;
        }
        
        public async Task<GetListResponse?> NotificationCenterGetList(Guid workspaceId, int page)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.NotificationCenterGetList, new GetListRequest()
            {
                WorkspaceId = workspaceId,
                Page = page
            });
        }
        
        public async Task NotificationCenterMarkAllAsRead(Guid workspaceId)
        {
            await PostAsync<GetListResponse>(ApiUrl.NotificationCenterMarkAllAsRead, new MarkAllAsReadRequest()
            {
                WorkspaceId = workspaceId
            });
        }
        
        public async Task NotificationCenterMarkAsRead(Guid notificationId)
        {
            await PostAsync<GetListResponse>(ApiUrl.NotificationCenterMarkAsRead, new MarkAsReadRequest()
            {
                NotificationId = notificationId
            });
        }
    }
}
