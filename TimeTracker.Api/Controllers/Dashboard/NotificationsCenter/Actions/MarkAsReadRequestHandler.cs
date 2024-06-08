using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.NotificationsCenter.Actions
{
    public class MarkAsReadRequestHandler : IAsyncRequestHandler<MarkAsReadRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly IMapper _mapper;

        public MarkAsReadRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            INotificationCenterService notificationCenterService,
            IDbSessionProvider sessionProvider,
            IMapper mapper
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _notificationCenterService = notificationCenterService;
            _sessionProvider = sessionProvider;
            _mapper = mapper;
        }
    
        public async Task ExecuteAsync(MarkAsReadRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var notification = await _sessionProvider.CurrentSession.GetAsync<NotificationEntity>(request.NotificationId);
            await _securityManager.CheckAccess(AccessLevel.Read, user, notification);
            await _notificationCenterService.MarkAsRead(notification);
        }
    }
}
