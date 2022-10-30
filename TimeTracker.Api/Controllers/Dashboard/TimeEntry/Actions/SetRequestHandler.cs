using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Actions
{
    public class SetRequestHandler : IAsyncRequestHandler<SetRequest, TimeEntryDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IProjectDao _projectDao;
        private readonly ITimeEntryDao _timeEntryDao;
        private readonly ITimeEntryService _timeEntryService;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly IProjectService _projectService;

        public SetRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IProjectDao projectDao,
            ITimeEntryDao timeEntryDao,
            ITimeEntryService timeEntryService,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            IProjectService projectService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _projectDao = projectDao;
            _timeEntryDao = timeEntryDao;
            _timeEntryService = timeEntryService;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _projectService = projectService;
        }
    
        public async Task<TimeEntryDto> ExecuteAsync(SetRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var timeEntry = await _timeEntryDao.GetByIdAsync(request.Id);
            if (timeEntry != null && !await _securityManager.HasAccess(AccessLevel.Write, user, timeEntry))
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
            
            timeEntry = await _timeEntryService.SetAsync(
                user,
                workspace,
                new TimeEntryCreationDto()
                {
                    Id = timeEntry?.Id,
                    Description = request.Description, 
                    TaskId = request.TaskId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    HourlyRate = request.HourlyRate,
                    IsBillable = request.IsBillable,
                    Date = request.Date
                },
                userProjects.Items.FirstOrDefault(item => item.Id == request.ProjectId)
            );
            return _mapper.Map<TimeEntryDto>(timeEntry);
        }
    }
}
