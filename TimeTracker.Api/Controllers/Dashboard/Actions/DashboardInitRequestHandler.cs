using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Services.Security;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Init;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Actions;

public class DashboardInitRequestHandler : IAsyncRequestHandler<DashboardInitRequest, DashboardInitResponse>
{
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IProjectDao _projectDao;
    private readonly IClientDao _clientDao;
    private readonly ITagDao _tagDao;
    private readonly ITaskListDao _taskListDao;
    private readonly ITaskDao _taskDao;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IClientPermissionService _clientPermissionService;
    private readonly IUserDtoBuilder _userDtoBuilder;

    public DashboardInitRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IWorkspaceDao workspaceDao,
        IProjectDao projectDao,
        IClientDao clientDao,
        ITagDao tagDao,
        ITaskListDao taskListDao,
        ITaskDao taskDao,
        ITimeEntryDao timeEntryDao,
        ISecurityManager securityManager,
        IWorkspaceAccessService workspaceAccessService,
        IClientPermissionService clientPermissionService,
        IUserDtoBuilder userDtoBuilder
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _workspaceDao = workspaceDao;
        _projectDao = projectDao;
        _clientDao = clientDao;
        _tagDao = tagDao;
        _taskListDao = taskListDao;
        _taskDao = taskDao;
        _timeEntryDao = timeEntryDao;
        _securityManager = securityManager;
        _workspaceAccessService = workspaceAccessService;
        _clientPermissionService = clientPermissionService;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<DashboardInitResponse> ExecuteAsync(DashboardInitRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspaceId = _apiRequestService.GetCurrentWorkspaceId();
        var workspace = workspaceId.HasValue
            ? await _userDao.GetUsersWorkspace(user, workspaceId.Value)
            : await _userDao.GetDefaultWorkspace(user);
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);

        // 1. Current user
        var currentUserDto = await _userDtoBuilder.BuildAsync(user);

        // 2. All workspaces with current user access
        var allWorkspaces = await _userDao.GetUsersWorkspaces(user);
        var workspaceDtos = _mapper.Map<ICollection<WorkspaceDto>>(allWorkspaces);
        foreach (var workspaceDto in workspaceDtos)
        {
            var ws = allWorkspaces.First(item => item.Id == workspaceDto.Id);
            workspaceDto.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, ws);
            workspaceDto.IsCreatedByCurrentUser = ws.CreatedUser.Id == user.Id;
        }

        var currentWorkspaceDto = workspaceDtos.FirstOrDefault(w => w.Id == workspace.Id)
            ?? _mapper.Map<WorkspaceDto>(workspace);

        // 3. Permissions
        var permissions = await _clientPermissionService.GetPermissionsAsync(user, workspace);

        // 4. Workspace members
        ICollection<WorkspaceMemberDto> members = new List<WorkspaceMemberDto>();
        if (workspace.Mode == WorkspaceMode.Team && await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
        {
            var membersDto = await _workspaceDao.GetMembersAsync(workspace, 1);
            members = _mapper.Map<ICollection<WorkspaceMemberDto>>(membersDto.Items);
        }

        // 5. Projects
        var projectListDto = await _projectDao.GetAvailableForUserListAsync(workspace, user, userAccess);
        var projects = _mapper.Map<ICollection<ProjectDto>>(projectListDto.Items);

        // 6. Clients
        var clientListDto = await _clientDao.GetListAsync(workspace, 1, user, userAccess);
        var clients = _mapper.Map<ICollection<ClientDto>>(clientListDto.Items);

        // 7. Tags
        var tagEntities = await _tagDao.GetList(workspace);
        var tags = _mapper.Map<ICollection<TagDto>>(tagEntities);

        // 8. Task lists
        var taskListsDto = await _taskListDao.GetAvailableForUserListAsync(workspace, user, userAccess);
        var taskListItems = _mapper.Map<ICollection<TaskListForListDto>>(taskListsDto.Items);
        var tasksCountMap = await _taskDao.GetTasksCountByTaskListIds(taskListsDto.Items.ToList());
        foreach (var item in taskListItems)
        {
            item.TasksCount = tasksCountMap.TryGetValue(item.Id, out var tasksCount)
                ? tasksCount
                : 0;
        }

        // 9. Active time entry
        var activeEntry = await _timeEntryDao.GetActiveEntryAsync(workspace, user);
        var activeTimeEntryDto = _mapper.Map<TimeEntryDto>(activeEntry);

        return new DashboardInitResponse
        {
            CurrentUser = currentUserDto,
            Workspaces = workspaceDtos,
            CurrentWorkspace = currentWorkspaceDto,
            Permissions = permissions,
            WorkspaceMembers = members,
            Projects = projects,
            Clients = clients,
            Tags = tags,
            TaskLists = taskListItems,
            ActiveTimeEntry = activeTimeEntryDto
        };
    }
}
