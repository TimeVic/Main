using AutoMapper;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Notes.Actions;

public abstract class NoteRequestHandlerBase
{
    protected class WorkspaceContext
    {
        public required UserEntity User { get; set; }

        public required WorkspaceEntity Workspace { get; set; }
    }

    protected readonly IMapper Mapper;
    protected readonly IApiRequestService ApiRequestService;
    protected readonly IUserDao UserDao;
    protected readonly ISecurityManager SecurityManager;
    protected readonly INoteService NoteService;

    protected NoteRequestHandlerBase(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ISecurityManager securityManager,
        INoteService noteService
    )
    {
        Mapper = mapper;
        ApiRequestService = apiRequestService;
        UserDao = userDao;
        SecurityManager = securityManager;
        NoteService = noteService;
    }

    protected async Task<WorkspaceContext> GetWorkspaceContextAsync(AccessLevel accessLevel = AccessLevel.Read)
    {
        var user = await ApiRequestService.GetCurrentUser();
        var workspaceId = ApiRequestService.GetCurrentWorkspaceId();
        if (!workspaceId.HasValue)
        {
            throw new RecordNotFoundException("Workspace not found");
        }

        var workspace = await UserDao.GetUsersWorkspace(user!, workspaceId);
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        if (!await SecurityManager.HasAccess(accessLevel, user, workspace))
        {
            throw new HasNoAccessException();
        }

        return new WorkspaceContext
        {
            User = user,
            Workspace = workspace
        };
    }
}
