using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Client.Actions
{
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, ClientDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IClientDao _clientDao;
        private readonly ISecurityManager _securityManager;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IClientDao clientDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _clientDao = clientDao;
            _securityManager = securityManager;
        }
    
        public async Task<ClientDto> ExecuteAsync(AddRequest request)
        {
            var userId = _apiRequestService.GetCurrentUserId();
            var user = await _userDao.GetById(userId);
            RecordNotFoundException.ThrowIfNull(user);
            var workspace = await _userDao.GetUsersWorkspace(user!, _apiRequestService.GetCurrentWorkspaceId());
            RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var client = await _clientDao.CreateAsync(workspace, request.Name);
            return _mapper.Map<ClientDto>(client);
        }
    }
}
