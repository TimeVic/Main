using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class StartRequestHandler : IAsyncRequestHandler<StartRequest, TimeEntryDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ISecurityManager _securityManager;
        private readonly IProjectService _projectService;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public StartRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ITimeEntryDao timeEntryDao,
            ISecurityManager securityManager,
            IProjectService projectService,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _timeEntryDao = timeEntryDao;
            _securityManager = securityManager;
            _projectService = projectService;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<TimeEntryDto> ExecuteAsync(StartRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            var userProjects = await _projectDao.GetAvailableForUserListAsync(workspace, user, userAccess);
            var project = userProjects.Items.FirstOrDefault(item => item.Id == request.ProjectId);
            if (request.IsBillable && !request.HourlyRate.HasValue)
            {
                request.HourlyRate = await _projectService.GetUsersHourlyRateForProject(user, project);
            }

            var timeEntry = await _timeEntryDao.StartNewAsync(
                user,
                workspace,
                request.Date,
                request.StartTime,
                isBillable: request.IsBillable,
                description: request.Description,
                projectId: request.ProjectId,
                hourlyRate: request.HourlyRate
            );
            await _sessionProvider.PerformCommitAsync();
            
            return _mapper.Map<TimeEntryDto>(timeEntry);
        }
    }
}
