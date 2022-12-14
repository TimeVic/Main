using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, WorkspaceDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<WorkspaceDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _workspaceDao.CreateWorkspaceAsync(user, request.Name);
            await _workspaceAccessService.ShareAccessAsync(workspace, user, MembershipAccessType.Owner);
            var response = _mapper.Map<WorkspaceDto>(workspace);
            response.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            return response;
        }
    }
}
