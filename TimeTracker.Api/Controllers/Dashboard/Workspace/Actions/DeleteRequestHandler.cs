using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Workspace;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions;

public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IWorkspaceDeletionService _workspaceDeletionService;

    public DeleteRequestHandler(
        IApiRequestService apiRequestService,
        IWorkspaceDao workspaceDao,
        IWorkspaceAccessService workspaceAccessService,
        IWorkspaceDeletionService workspaceDeletionService
    )
    {
        _apiRequestService = apiRequestService;
        _workspaceDao = workspaceDao;
        _workspaceAccessService = workspaceAccessService;
        _workspaceDeletionService = workspaceDeletionService;
    }

    public async Task ExecuteAsync(DeleteRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _workspaceDao.GetById(request.WorkspaceId);
        RecordNotFoundException.ThrowIfNull(workspace);

        var access = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        if (access != MembershipAccessType.Owner)
        {
            throw new HasNoAccessException();
        }

        if (!string.Equals(workspace.Name, request.ConfirmationName.Trim(), StringComparison.Ordinal))
        {
            throw new DataValidationException("Workspace name confirmation does not match.");
        }

        await _workspaceDeletionService.SoftDeleteAsync(workspace);
    }
}
