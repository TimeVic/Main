using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Client.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, ClientDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IClientDao _clientDao;
        private readonly ISecurityManager _securityManager;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IClientDao clientDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _clientDao = clientDao;
            _securityManager = securityManager;
        }
    
        public async Task<ClientDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var client = await _clientDao.GetById(request.Id);
            if (client == null)
            {
                throw new RecordNotFoundException();
            }
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, client.Workspace))
            {
                throw new HasNoAccessException();
            }
            client.Name = request.Name;
            return _mapper.Map<ClientDto>(client);
        }
    }
}
