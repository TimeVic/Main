using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions;

public class SetModeRequestHandler : IAsyncRequestHandler<SetModeRequest, WorkspaceDto>
{
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly ISecurityManager _securityManager;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public SetModeRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IWorkspaceDao workspaceDao,
        ISecurityManager securityManager,
        IWorkspaceAccessService workspaceAccessService
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _workspaceDao = workspaceDao;
        _securityManager = securityManager;
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<WorkspaceDto> ExecuteAsync(SetModeRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspaceId = _apiRequestService.GetCurrentWorkspaceId();
        var workspace = await _workspaceDao.GetById(workspaceId);
        RecordNotFoundException.ThrowIfNull(workspace);

        if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
        {
            throw new HasNoAccessException();
        }

        if (workspace.Mode.HasValue)
        {
            throw new HasNoAccessException("Workspace mode has already been set and cannot be changed.");
        }

        workspace = await _workspaceDao.SetModeAsync(workspace, request.Mode);
        var response = _mapper.Map<WorkspaceDto>(workspace);
        response.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
        return response;
    }
}
