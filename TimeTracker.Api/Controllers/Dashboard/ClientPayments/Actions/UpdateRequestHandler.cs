using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.ClientPayments.Actions;

public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, ClientPaymentDto>
{
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly IClientPaymentDao _paymentDao;
    private readonly IClientDao _clientDao;
    private readonly ISecurityManager _securityManager;

    public UpdateRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        IClientPaymentDao paymentDao,
        IClientDao clientDao,
        ISecurityManager securityManager
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _paymentDao = paymentDao;
        _clientDao = clientDao;
        _securityManager = securityManager;
    }

    public async Task<ClientPaymentDto> ExecuteAsync(UpdateRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var payment = await _paymentDao.GetById(request.ClientPaymentId);
        RecordNotFoundException.ThrowIfNull(payment);
        if (!await _securityManager.HasAccess(AccessLevel.Write, user, payment))
        {
            throw new HasNoAccessException();
        }

        var client = await _clientDao.GetById(request.ClientId);
        RecordNotFoundException.ThrowIfNull(client);
        if (
            client.Workspace.Id != payment.Client.Workspace.Id
            || !await _securityManager.HasAccess(AccessLevel.Write, user, client)
        )
        {
            throw new HasNoAccessException();
        }

        payment = await _paymentDao.UpdateClientPaymentAsync(
            request.ClientPaymentId,
            client,
            request.Amount,
            request.PaymentTime,
            request.ProjectId,
            request.Description
        );
        return _mapper.Map<ClientPaymentDto>(payment);
    }
}
