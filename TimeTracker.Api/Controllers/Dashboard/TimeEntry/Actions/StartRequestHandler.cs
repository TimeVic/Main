using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class StartRequestHandler : IAsyncRequestHandler<StartRequest, StartResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly ISecurityManager _securityManager;
        private readonly IProjectService _projectService;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly ITaskDao _taskDao;
        private readonly ITimeEntryService _timeEntryService;

        public StartRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IProjectDao projectDao,
            IDbSessionProvider sessionProvider,
            ISecurityManager securityManager,
            IProjectService projectService,
            IWorkspaceAccessService workspaceAccessService,
            ITaskDao taskDao,
            ITimeEntryService timeEntryService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _sessionProvider = sessionProvider;
            _securityManager = securityManager;
            _projectService = projectService;
            _workspaceAccessService = workspaceAccessService;
            _taskDao = taskDao;
            _timeEntryService = timeEntryService;
        }
    
        public async Task<StartResponse> ExecuteAsync(StartRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
            var task = await _taskDao.GetById(request.InternalTaskId ?? Guid.Empty);
            if (task != null)
            {
                if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
                {
                    throw new HasNoAccessException();
                }

                if (task.Workspace.Id != workspace?.Id)
                {
                    throw new ValidationException("Provided TaskId from other workspace");
                }
            }

            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            // Stops the current entry and starts the new one in the request transaction.
            var stoppedTimeEntry = (await _timeEntryService.StopActiveAsync(workspace, user))
                .FirstOrDefault();

            var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            var userProjects = await _projectDao.GetAvailableForUserListAsync(workspace, user, userAccess);
            var project = userProjects.Items.FirstOrDefault(item => item.Id == request.ProjectId);

            var isBillable = request.IsBillable ?? project?.IsBillableByDefault ?? false;
            if (isBillable && !request.HourlyRate.HasValue)
            {
                request.HourlyRate = await _projectService.GetUsersHourlyRateForProject(user, project);
            }

            var timeEntry = await _timeEntryService.StartAsync(
                user,
                workspace,
                isBillable: isBillable,
                description: request.Description,
                projectId: request.ProjectId,
                hourlyRate: request.HourlyRate,
                internalTask: task
            );
            return new StartResponse
            {
                ActiveTimeEntry = _mapper.Map<TimeEntryDto>(timeEntry),
                StoppedTimeEntry = _mapper.Map<TimeEntryDto>(stoppedTimeEntry)
            };
        }
    }
}
