using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<int> NotificationCenterGetUnreadCount(Guid workspaceId)
        {
            var response = await PostAsync<GetCountResponse>(ApiUrl.NotificationCenterGetCount, new GetCountRequest()
            {
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
                Page = page
            });
        }
        
        public async Task NotificationCenterMarkAllAsRead(Guid workspaceId)
        {
            await PostAsync<GetListResponse>(ApiUrl.NotificationCenterMarkAllAsRead, new MarkAllAsReadRequest()
            {
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
