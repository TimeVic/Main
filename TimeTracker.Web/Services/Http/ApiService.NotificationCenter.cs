using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<int> NotificationCenterGetUnreadCount(long workspaceId)
        {
            var response = await PostAsync<GetCountResponse>(ApiUrl.NotificationCenterGetCount, new GetCountRequest()
            {
                WorkspaceId = workspaceId
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response.UnreadCount;
        }
        
        public async Task<GetListResponse> NotificationCenterGetList(long workspaceId, int page)
        {
            var response = await PostAsync<GetListResponse>(ApiUrl.NotificationCenterGetList, new GetListRequest()
            {
                WorkspaceId = workspaceId,
                Page = page
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task NotificationCenterMarkAllAsRead(long workspaceId)
        {
            await PostAsync<GetListResponse>(ApiUrl.NotificationCenterMarkAllAsRead, new MarkAllAsReadRequest()
            {
                WorkspaceId = workspaceId
            });
        }
        
        public async Task NotificationCenterMarkAsRead(long notificationId)
        {
            await PostAsync<GetListResponse>(ApiUrl.NotificationCenterMarkAsRead, new MarkAsReadRequest()
            {
                NotificationId = notificationId
            });
        }
    }
}
