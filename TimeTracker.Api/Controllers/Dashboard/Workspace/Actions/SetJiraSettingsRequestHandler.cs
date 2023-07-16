using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class SetJiraSettingsRequestHandler : IAsyncRequestHandler<SetJiraSettingsRequest, WorkspaceSettingsJiraDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
        private readonly IJiraClient _jiraClient;

        public SetJiraSettingsRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            IWorkspaceSettingsDao workspaceSettingsDao,
            IJiraClient jiraClient
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _workspaceSettingsDao = workspaceSettingsDao;
            _jiraClient = jiraClient;
        }
    
        public async Task<WorkspaceSettingsJiraDto> ExecuteAsync(SetJiraSettingsRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _workspaceDao.GetByIdAsync(request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var settings = await _workspaceSettingsDao.SetJiraAsync(
                user,
                workspace,
                request.Url,
                request.ApiKey,
                request.UserName,
                false
            );
            settings.IsActive = await _jiraClient.IsValidClientSettings(workspace, user);
            return _mapper.Map<WorkspaceSettingsJiraDto>(settings);
        }
    }
}
