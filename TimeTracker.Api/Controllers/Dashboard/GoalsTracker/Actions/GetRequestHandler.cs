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

namespace TimeTracker.Api.Controllers.Dashboard.GoalsTracker.Actions
{
    public class GetRequestHandler : IAsyncRequestHandler<GetRequest, GoalsTrackerDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IClientDao _clientDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IGoalsTrackerDao _goalsTrackerDao;

        public GetRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IClientDao clientDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IGoalsTrackerDao goalsTrackerDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _clientDao = clientDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _goalsTrackerDao = goalsTrackerDao;
        }
    
        public async Task<GoalsTrackerDto> ExecuteAsync(GetRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var goalsTracker = await _goalsTrackerDao.CheckAndCreate(user, workspace, request.Date);
            return _mapper.Map<GoalsTrackerDto>(goalsTracker);
        }
    }
}
