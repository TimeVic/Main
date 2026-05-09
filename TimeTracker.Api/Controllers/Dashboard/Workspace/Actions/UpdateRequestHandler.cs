using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, WorkspaceDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly ISecurityManager _securityManager;
        private readonly ICurrencyDao _currencyDao;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IWorkspaceDao workspaceDao,
            ISecurityManager securityManager,
            ICurrencyDao currencyDao,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _workspaceDao = workspaceDao;
            _securityManager = securityManager;
            _currencyDao = currencyDao;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<WorkspaceDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _workspaceDao.GetById(_apiRequestService.GetCurrentWorkspaceId());
            RecordNotFoundException.ThrowIfNull(workspace);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
            {
                throw new HasNoAccessException();
            }
            
            CurrencyEntity? currency = null;
            if (request.CurrencyId != Guid.Empty)
            {
                currency = await _currencyDao.GetBy(request.CurrencyId);
                RecordNotFoundException.ThrowIfNull(currency);
            }
            else
            {
                currency = workspace.Currency;
            }
            workspace = await _workspaceDao.UpdateWorkspaceAsync(
                workspace,
                request.Name,
                currency,
                request.TimeZone,
                request.Description
            );
            var response = _mapper.Map<WorkspaceDto>(workspace);
            response.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            return response;
        }
    }
}
