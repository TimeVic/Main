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
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateItemRequest, GoalsTrackerItemDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IClientDao _clientDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IGoalsTrackerDao _goalsTrackerDao;
        private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IClientDao clientDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IGoalsTrackerDao goalsTrackerDao,
            IGoalsTrackerItemsDao goalsTrackerItemsDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _clientDao = clientDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _goalsTrackerDao = goalsTrackerDao;
            _goalsTrackerItemsDao = goalsTrackerItemsDao;
        }
    
        public async Task<GoalsTrackerItemDto> ExecuteAsync(UpdateItemRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var goalsTrackerItem = await _goalsTrackerItemsDao.GetById(request.GoalsTrackerItemId);
            await _securityManager.CheckAccess(AccessLevel.Write, user, goalsTrackerItem?.Tracker);
            var trackerItem = await _goalsTrackerItemsDao.Update(goalsTrackerItem!, request.Name, request.NumberOfTimes);
            return _mapper.Map<GoalsTrackerItemDto>(trackerItem);
        }
    }
}
