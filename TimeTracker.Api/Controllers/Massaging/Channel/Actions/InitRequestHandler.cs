using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Api.WebSocket.Services;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Massaging.Channel.Actions
{
    public class InitRequestHandler : IAsyncRequestHandler<InitRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IHubMessagingService _hubMessagingService;

        public InitRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            IHubMessagingService hubMessagingService
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _hubMessagingService = hubMessagingService;
        }
    
        public async Task ExecuteAsync(InitRequest request)
        {
            var currentUser = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetById(request.WorkspaceId);
            DataValidationException.ThrowIfNull(workspace);
            await _securityManager.CheckAccess(AccessLevel.Read, currentUser, workspace);
            
            await _hubMessagingService.InitChannels(workspace, currentUser);
        }
    }
}
