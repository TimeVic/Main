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
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly INotificationCenterService _notificationCenterService;

        public GetCountRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            INotificationCenterService notificationCenterService
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _notificationCenterService = notificationCenterService;
        }
    
        public async Task<GetCountResponse> ExecuteAsync(GetCountRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            await _securityManager.CheckAccess(AccessLevel.Read, user, workspace);

            return new GetCountResponse()
            {
                UnreadCount = await _notificationCenterService.GetUnreadCount(user, workspace)
            };
        }
    }
}
