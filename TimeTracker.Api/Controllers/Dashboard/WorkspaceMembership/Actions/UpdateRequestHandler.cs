using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.WorkspaceMembership.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, WorkspaceMembershipDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly IWorkspaceDao _workspaceDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            IWorkspaceDao workspaceDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _workspaceDao = workspaceDao;
        }
    
        public async Task<WorkspaceMembershipDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var membership = await _workspaceDao.GetMembershipAsync(request.MembershipId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, membership.Workspace))
            {
                throw new HasNoAccessException();
            }

            var projects = new List<ProjectEntity>();
            if (request.ProjectIds != null)
            {
                projects = membership.Workspace.Projects.Where(
                    item => request.ProjectIds.Contains(item.Id)
                ).ToList();    
            }
            var workspaceMembership = await _workspaceAccessService.ShareAccessAsync(
                membership.Workspace,
                user,
                request.Access,
                projects
            );
            return _mapper.Map<WorkspaceMembershipDto>(workspaceMembership);
        }
    }
}
