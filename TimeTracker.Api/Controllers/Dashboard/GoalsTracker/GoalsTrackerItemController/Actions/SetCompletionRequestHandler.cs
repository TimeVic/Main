using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.GoalsTracker.GoalsTrackerItemController.Actions
{
    public class SetCompletionRequestHandler : IAsyncRequestHandler<SetCompletionRequest, GoalsTrackerCompletionMarkerDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;

        public SetCompletionRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IGoalsTrackerItemsDao goalsTrackerItemsDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _goalsTrackerItemsDao = goalsTrackerItemsDao;
        }
    
        public async Task<GoalsTrackerCompletionMarkerDto> ExecuteAsync(SetCompletionRequest request)
        {
            var userId = _apiRequestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var goalsTrackerItem = await _goalsTrackerItemsDao.GetById(request.GoalsTrackerItemId);
            await _securityManager.CheckAccess(AccessLevel.Write, user, goalsTrackerItem?.Tracker);
            var completionMarker = await _goalsTrackerItemsDao.SetCompletion(
                goalsTrackerItem,
                request.DayOfMonth,
                request.IsChecked
            );
            return _mapper.Map<GoalsTrackerCompletionMarkerDto>(completionMarker);
        }
    }
}
