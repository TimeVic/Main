using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, WorkspaceDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IWorkspaceDao workspaceDao,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _workspaceDao = workspaceDao;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<WorkspaceDto> ExecuteAsync(AddRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var createdWorkspacesCount = await _workspaceDao.GetActiveCreatedWorkspacesCountAsync(user);
            if (createdWorkspacesCount >= GlobalConstants.MaxActiveCreatedWorkspaces)
            {
                throw new DataValidationException($"You can create up to {GlobalConstants.MaxActiveCreatedWorkspaces} active workspaces.");
            }

            var workspace = await _workspaceDao.CreateWorkspaceAsync(user, request.Name);
            await _workspaceAccessService.ShareAccessAsync(workspace, user, MembershipAccessType.Owner);
            var response = _mapper.Map<WorkspaceDto>(workspace);
            response.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            response.IsCreatedByCurrentUser = true;
            return response;
        }
    }
}
