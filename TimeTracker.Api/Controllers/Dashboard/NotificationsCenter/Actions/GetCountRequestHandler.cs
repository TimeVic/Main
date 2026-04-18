using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.NotificationsCenter.Actions
{
    public class GetCountRequestHandler : IAsyncRequestHandler<GetCountRequest, GetCountResponse>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly INotificationCenterService _notificationCenterService;

        public GetCountRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            INotificationCenterService notificationCenterService
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _notificationCenterService = notificationCenterService;
        }
    
        public async Task<GetCountResponse> ExecuteAsync(GetCountRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);

            return new GetCountResponse()
            {
                UnreadCount = await _notificationCenterService.GetUnreadCount(user, workspace!)
            };
        }
    }
}
