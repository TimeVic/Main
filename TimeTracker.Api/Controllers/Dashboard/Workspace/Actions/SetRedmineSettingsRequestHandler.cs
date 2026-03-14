using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class SetRedmineSettingsRequestHandler : IAsyncRequestHandler<SetRedmineSettingsRequest, WorkspaceSettingsRedmineDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
        private readonly IRedmineClient _redmineClient;

        public SetRedmineSettingsRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            IWorkspaceSettingsDao workspaceSettingsDao,
            IRedmineClient redmineClient
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _workspaceSettingsDao = workspaceSettingsDao;
            _redmineClient = redmineClient;
        }
    
        public async Task<WorkspaceSettingsRedmineDto> ExecuteAsync(SetRedmineSettingsRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetById(request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }
            var settings = await _workspaceSettingsDao.SetRedmineAsync(
                user,
                workspace,
                request.Url,
                request.ApiKey,
                request.RedmineUserId,
                request.ActivityId
            );

            settings.IsActive = await _redmineClient.IsValidClientSettings(workspace, user);
            return _mapper.Map<WorkspaceSettingsRedmineDto>(settings);
        }
    }
}
