using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions
{
    public class SetNotificationTokenRequestHandler : IAsyncRequestHandler<SetNotificationTokenRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IUserNotificationTokenDao _userNotificationTokenDao;

        public SetNotificationTokenRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            IUserNotificationTokenDao userNotificationTokenDao
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _userNotificationTokenDao = userNotificationTokenDao;
        }
    
        public async Task ExecuteAsync(SetNotificationTokenRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            await _userNotificationTokenDao.Set(user, request.Token);
        }
    }
}
