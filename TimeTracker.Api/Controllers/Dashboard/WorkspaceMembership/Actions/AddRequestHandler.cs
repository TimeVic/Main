using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.WorkspaceMembership.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, WorkspaceMembershipDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<WorkspaceMembershipDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var workspaceMembership = await _workspaceAccessService.ShareAccessAsync(
                workspace,
                request.Email,
                MembershipAccessType.User
            );
            return _mapper.Map<WorkspaceMembershipDto>(workspaceMembership);
        }
    }
}
