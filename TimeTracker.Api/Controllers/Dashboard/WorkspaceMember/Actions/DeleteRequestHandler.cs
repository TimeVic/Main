using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.WorkspaceMember.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;
        private readonly IWorkspaceDao _workspaceDao;

        public DeleteRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService,
            IWorkspaceDao workspaceDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
            _workspaceDao = workspaceDao;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var member = await _workspaceDao.GetMemberAsync(request.MemberId);
            if (member?.Workspace.Mode != WorkspaceMode.Team || !await _securityManager.HasAccess(AccessLevel.Write, user, member.Workspace))
            {
                throw new HasNoAccessException();
            }

            await _workspaceAccessService.RemoveAccessAsync(request.MemberId);
        }
    }
}
