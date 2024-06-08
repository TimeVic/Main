using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.GoalsTracker.Actions
{
    public class ChangePositionsRequestHandler : IAsyncRequestHandler<ChangePositionsRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IGoalsTrackerDao _goalsTrackerDao;

        public ChangePositionsRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IGoalsTrackerDao goalsTrackerDao
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _goalsTrackerDao = goalsTrackerDao;
        }
    
        public async Task ExecuteAsync(ChangePositionsRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException("Workspace not found");
            }
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var goalsTracker = await _goalsTrackerDao.CheckAndCreate(user, workspace, request.Date);
            foreach (var (goalId, position) in request.Positions)
            {
                foreach (var goal in goalsTracker.Items)
                {
                    if (goal.Id == goalId)
                    {
                        goal.Position = position;
                    }
                }
            }
        }
    }
}
