using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class GetSettingsRequestHandler : IAsyncRequestHandler<GetIntegrationSettingsRequest, GetIntegrationSettingsResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
        private readonly IRedmineClient _redmineClient;

        public GetSettingsRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            IWorkspaceSettingsDao workspaceSettingsDao,
            IRedmineClient redmineClient
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _workspaceSettingsDao = workspaceSettingsDao;
            _redmineClient = redmineClient;
        }
    
        public async Task<GetIntegrationSettingsResponse> ExecuteAsync(GetIntegrationSettingsRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = await _workspaceDao.GetByIdAsync(request.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var redmineSettings = workspace.GetRedmineSettings(user.Id);
            var clickUpSettings = workspace.GetClickUpSettings(user.Id);
            return new GetIntegrationSettingsResponse()
            {
                IntegrationRedmine = _mapper.Map<WorkspaceSettingsRedmineDto>(redmineSettings),
                IntegrationClickUp = _mapper.Map<WorkspaceSettingsClickUpDto>(clickUpSettings),
            };
        }
    }
}
