using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;
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

namespace TimeTracker.Api.Controllers.Massaging.Message.Actions
{
    public class SendRequestHandler : IAsyncRequestHandler<SendRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IMessagingDao _messagingDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IHubMessagingService _hubMessagingService;

        public SendRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IMessagingDao messagingDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            IHubMessagingService hubMessagingService
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _messagingDao = messagingDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _hubMessagingService = hubMessagingService;
        }
    
        public async Task ExecuteAsync(SendRequest request)
        {
            UserEntity? receiver = null;
            MessagingChannelEntity? channel = null;
            
            var currentUser = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetById(request.WorkspaceId);
            DataValidationException.ThrowIfNull(workspace);
            await _securityManager.CheckAccess(AccessLevel.Read, currentUser, workspace);

            if (request.ReceiverId != null)
            {
                receiver = await _userDao.GetById(request.ReceiverId.Value);
                DataValidationException.ThrowIfNull(receiver);
                await _securityManager.CheckAccess(AccessLevel.Read, receiver, workspace);
            }
            else if (request.ChannelId != null)
            {
                channel = await _messagingDao.GetChannelBy(request.ChannelId.Value);
                DataValidationException.ThrowIfNull(channel);
                await _securityManager.CheckAccess(AccessLevel.Read, currentUser, channel);
            }
            else
            {
                throw new DataValidationException("ChannelId or ReceiverId are required");
            }
        
            await _hubMessagingService.SendMessage(
                workspace,
                currentUser, 
                request.Text,
                receiver,
                channel
            );
        }
    }
}
