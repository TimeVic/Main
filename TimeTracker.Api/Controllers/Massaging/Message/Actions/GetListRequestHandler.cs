using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Massaging.Message.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly ISecurityManager _securityManager;
        private readonly IMessagingDao _messagingDao;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            ISecurityManager securityManager,
            IMessagingDao messagingDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _securityManager = securityManager;
            _messagingDao = messagingDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var currentUser = await _apiRequestService.GetCurrentUser();
            var channel = await _messagingDao.GetChannelBy(request.ChannelId);
            DataValidationException.ThrowIfNull(channel);
            await _securityManager.CheckAccess(AccessLevel.Write, currentUser, channel);

            var listResponse = await _messagingDao.GetMessagesList(channel, request.Page);
            return new GetListResponse(
                _mapper.Map<ICollection<MessagingMessageDto>>(listResponse.Items),
                listResponse.TotalCount
            );
        }
    }
}
