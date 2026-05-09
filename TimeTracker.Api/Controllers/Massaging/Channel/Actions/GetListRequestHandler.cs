using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Massaging.Channel.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly IMessagingDao _messagingDao;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao,
            IMessagingDao messagingDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
            _messagingDao = messagingDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var currentUser = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetById(_apiRequestService.GetCurrentWorkspaceId());
            DataValidationException.ThrowIfNull(workspace);
            await _securityManager.CheckAccess(AccessLevel.Read, currentUser, workspace);

            var channels = await _messagingDao.GetChannelsList(workspace, currentUser);
            return new GetListResponse(
                _mapper.Map<ICollection<MessagingChannelDto>>(channels),
                channels.Count
            );
        }
    }
}
