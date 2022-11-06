using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Auth;
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
        private readonly IRegistrationService _registrationService;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            IRegistrationService registrationService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _registrationService = registrationService;
        }
    
        public async Task<WorkspaceMembershipDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var currentUser = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(currentUser, request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, currentUser, workspace))
            {
                throw new HasNoAccessException();
            }
            var user = await _userDao.GetByEmail(request.Email);
            if (user is not { IsActivated: true })
            {
                user = await _registrationService.CreatePendingUser(request.Email);
            }
            else
            {
                var userAccessLevel = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
                if (userAccessLevel != null)
                {
                    throw new RecordIsExistsException("This membership already added");
                }    
            }
            var workspaceMembership = await _workspaceAccessService.ShareAccessAsync(
                workspace,
                user,
                MembershipAccessType.User
            );
            return _mapper.Map<WorkspaceMembershipDto>(workspaceMembership);
        }
    }
}
