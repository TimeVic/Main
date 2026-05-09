using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.ClientPayments.Actions;

public class AddRequestHandler : IAsyncRequestHandler<AddRequest, ClientPaymentDto>
{
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly IClientPaymentDao _paymentDao;
    private readonly IClientDao _clientDao;
    private readonly ISecurityManager _securityManager;

    public AddRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IUserDao userDao,
        IClientPaymentDao paymentDao,
        IClientDao clientDao,
        ISecurityManager securityManager
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _paymentDao = paymentDao;
        _clientDao = clientDao;
        _securityManager = securityManager;
    }

    public async Task<ClientPaymentDto> ExecuteAsync(AddRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var workspace = await _userDao.GetUsersWorkspace(user, request.WorkspaceId);
        RecordNotFoundException.ThrowIfNull(workspace, nameof(request.WorkspaceId));
        if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
        {
            throw new HasNoAccessException();
        }

        var client = await _clientDao.GetById(request.ClientId);
        RecordNotFoundException.ThrowIfNull(client);
        if (
            client.Workspace.Id != workspace.Id
            || !await _securityManager.HasAccess(AccessLevel.Write, user, client)
        )
        {
            throw new HasNoAccessException();
        }

        var payment = await _paymentDao.CreateAsync(
            workspace,
            client,
            request.Amount,
            request.PaymentTime,
            request.ProjectId,
            request.Description
        );
        return _mapper.Map<ClientPaymentDto>(payment);
    }
}
