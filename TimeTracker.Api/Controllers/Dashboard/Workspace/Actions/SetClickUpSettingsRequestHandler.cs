using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class SetClickUpSettingsRequestHandler : IAsyncRequestHandler<SetClickUpSettingsRequest, WorkspaceSettingsClickUpDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
        private readonly IClickUpClient _clickUpClient;

        public SetClickUpSettingsRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            IWorkspaceSettingsDao workspaceSettingsDao,
            IClickUpClient clickUpClient
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _workspaceSettingsDao = workspaceSettingsDao;
            _clickUpClient = clickUpClient;
        }
    
        public async Task<WorkspaceSettingsClickUpDto> ExecuteAsync(SetClickUpSettingsRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _workspaceDao.GetByIdAsync(request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var settings = await _workspaceSettingsDao.SetClickUpAsync(
                user,
                workspace,
                request.SecurityKey,
                request.TeamId,
                request.IsCustomTaskIds,
                request.IsFillTimeEntryWithTaskDetails
            );
            settings.IsActive = await _clickUpClient.IsValidClientSettings(workspace, user);
            return _mapper.Map<WorkspaceSettingsClickUpDto>(settings);
        }
    }
}
