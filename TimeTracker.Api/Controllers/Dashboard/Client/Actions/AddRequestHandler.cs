using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.Client.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, ClientDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IClientDao _clientDao;
        private readonly IDbSessionProvider _sessionProvider;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IClientDao clientDao,
            IDbSessionProvider sessionProvider
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _clientDao = clientDao;
            _sessionProvider = sessionProvider;
        }
    
        public async Task<ClientDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var workspace = user.Workspaces.FirstOrDefault(item => item.Id == request.WorkspaceId);
            if (workspace == null)
            {
                throw new RecordNotFoundException("Workspace not found");
            }
            var client = await _clientDao.CreateAsync(workspace, request.Name);
            await _sessionProvider.PerformCommitAsync();
            
            return _mapper.Map<ClientDto>(client);
        }
    }
}
