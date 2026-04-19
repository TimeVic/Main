using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions
{
    public class SetNotificationTokenRequestHandler : IAsyncRequestHandler<SetNotificationTokenRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserNotificationTokenDao _userNotificationTokenDao;

        public SetNotificationTokenRequestHandler(
            IApiRequestService apiRequestService,
            IUserNotificationTokenDao userNotificationTokenDao
        )
        {
            _apiRequestService = apiRequestService;
            _userNotificationTokenDao = userNotificationTokenDao;
        }
    
        public async Task ExecuteAsync(SetNotificationTokenRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            await _userNotificationTokenDao.Set(user, request.Token);
        }
    }
}
