using Api.Requests.Abstractions;
using AutoMapper;
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
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IClientDao _clientDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IClientDao clientDao,
            ISecurityManager securityManager,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _clientDao = clientDao;
            _securityManager = securityManager;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            var listDto = await _clientDao.GetListAsync(workspace, request.Page, user, accessType);
            return new GetListResponse(
                _mapper.Map<ICollection<ClientDto>>(listDto.Items),
                listDto.TotalCount
            );
        }
    }
}
