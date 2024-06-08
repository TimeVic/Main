using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.NotificationsCenter.Actions
{
    public class MarkAllAsReadRequestHandler : IAsyncRequestHandler<MarkAllAsReadRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly IMapper _mapper;

        public MarkAllAsReadRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            INotificationCenterService notificationCenterService,
            IMapper mapper
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _notificationCenterService = notificationCenterService;
            _mapper = mapper;
        }
    
        public async Task ExecuteAsync(MarkAllAsReadRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            await _notificationCenterService.MarkAllAsRead(user, workspace);
        }
    }
}
