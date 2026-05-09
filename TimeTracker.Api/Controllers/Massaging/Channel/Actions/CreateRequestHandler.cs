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
    public class CreateRequestHandler : IAsyncRequestHandler<CreateRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly IHubMessagingService _hubMessagingService;

        public CreateRequestHandler(
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
    
        public async Task ExecuteAsync(CreateRequest request)
        {

            if (request.MemberIds.Count > 50)
                throw new DataValidationException("Too many members");
            
            var currentUser = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetById(_apiRequestService.GetCurrentWorkspaceId());
            DataValidationException.ThrowIfNull(workspace);
            await _securityManager.CheckAccess(AccessLevel.Read, currentUser, workspace);

            var members = new List<UserEntity>();
            foreach (var memberId in request.MemberIds)
            {
                var member = await _userDao.GetById(memberId);
                if (member is null)
                {
                    throw new DataValidationException($"User with id {memberId} not found");
                }
                await _securityManager.CheckAccess(AccessLevel.Read, member, workspace);
                members.Add(member);
            }
            
            await _hubMessagingService.CreateChannel(workspace, currentUser, request.Slug, members);
        }
    }
}
