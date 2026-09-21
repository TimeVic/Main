using System.ComponentModel;
using AutoMapper;
using ModelContextProtocol.Server;
using TimeTracker.Api.Mcp.Context;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model.Mcp.Context;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Mcp.Tools;

[McpServerToolType]
public sealed class ContextTools
{
    private readonly IMcpContextService _mcpContext;
    private readonly IUserDao _userDao;
    private readonly IProjectDao _projectDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IMapper _mapper;

    public ContextTools(
        IMcpContextService mcpContext,
        IUserDao userDao,
        IProjectDao projectDao,
        ISecurityManager securityManager,
        IWorkspaceAccessService workspaceAccessService,
        IMapper mapper
    )
    {
        _mcpContext = mcpContext;
        _userDao = userDao;
        _projectDao = projectDao;
        _securityManager = securityManager;
        _workspaceAccessService = workspaceAccessService;
        _mapper = mapper;
    }

    [McpServerTool(Name = "get_user_context", ReadOnly = true)]
    [Description("Retrieves conversation context: current user profile, all accessible workspaces (currencies, settings), and available projects across all workspaces with their workspace associations. Call this at the start of a conversation to establish context.")]
    public async Task<UserContextDto> GetUserContext()
    {
        var user = await _mcpContext.GetUserAsync();
        var workspaces = await _userDao.GetUsersWorkspaces(user);

        var workspaceDtos = new List<WorkspaceDto>();
        var allProjects = new List<ProjectDto>();

        foreach (var workspace in workspaces)
        {
            var access = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            var workspaceDto = _mapper.Map<WorkspaceDto>(workspace);
            workspaceDto.CurrentUserAccess = access;
            workspaceDtos.Add(workspaceDto);

            var projectList = await _projectDao.GetAvailableForUserListAsync(workspace, user, access);
            var mappedProjects = _mapper.Map<ICollection<ProjectDto>>(projectList.Items);
            foreach (var project in mappedProjects)
            {
                allProjects.Add(project);
            }
        }

        return new UserContextDto
        {
            User = _mapper.Map<UserDto>(user),
            Workspaces = workspaceDtos,
            Projects = allProjects,
            CurrentTimeUtc = DateTime.UtcNow
        };
    }

    [McpServerTool(Name = "project_list", ReadOnly = true)]
    [Description("Retrieves the list of projects available to the user, optionally filtered by workspace.")]
    public async Task<GetListResponse> GetList(
        [Description("Workspace ID (optional). If omitted, returns projects across all accessible workspaces.")] Guid? workspaceId = null
    )
    {
        if (workspaceId.HasValue)
        {
            var (user, workspace) = await _mcpContext.GetUserAndWorkspaceAsync(workspaceId.Value);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var userAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            var listDto = await _projectDao.GetAvailableForUserListAsync(workspace, user, userAccess);
            return new GetListResponse(
                _mapper.Map<ICollection<ProjectDto>>(listDto.Items),
                listDto.TotalCount
            );
        }
        else
        {
            var context = await GetUserContext();
            return new GetListResponse(
                context.Projects,
                context.Projects.Count
            );
        }
    }
}

