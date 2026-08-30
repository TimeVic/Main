using TimeTracker.Business.Orm.Entities.User;
using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.WorkspaceMember.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, WorkspaceMemberDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly IRegistrationService _registrationService;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            IRegistrationService registrationService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _registrationService = registrationService;
        }
    
        public async Task<WorkspaceMemberDto> ExecuteAsync(AddRequest request)
        {
            var currentUser = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(currentUser, _apiRequestService.GetCurrentWorkspaceId());
            if (workspace?.Mode != WorkspaceMode.Team || !await _securityManager.HasAccess(AccessLevel.Write, currentUser, workspace))
            {
                throw new HasNoAccessException();
            }

            UserEntity? user;
            var cleanInput = request.Email.Trim().TrimStart('@');
            if (request.Email.Contains('@'))
            {
                user = await _userDao.GetByEmail(request.Email.Trim());
                if (user is not { IsActivated: true })
                {
                    user = await _registrationService.CreatePendingUser(request.Email.Trim());
                }
            }
            else
            {
                user = await _userDao.GetByLogin(cleanInput);
                if (user == null)
                {
                    throw new RecordNotFoundException("User with this login not found");
                }
            }

            var userAccessLevel = await _workspaceAccessService.GetAccessTypeAsync(user, workspace!);
            if (userAccessLevel != null)
            {
                throw new RecordIsExistsException("This member already added");
            }

            var access = request.Access == MembershipAccessType.Manager
                ? MembershipAccessType.Manager
                : MembershipAccessType.User;

            var workspaceMember = await _workspaceAccessService.ShareAccessAsync(
                workspace!,
                user,
                access
            );
            return _mapper.Map<WorkspaceMemberDto>(workspaceMember);
        }
    }
}
