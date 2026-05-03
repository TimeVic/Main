using Api.Requests.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.ClientPayments.Actions;

public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IClientPaymentDao _paymentDao;
    private readonly ISecurityManager _securityManager;

    public DeleteRequestHandler(
        IApiRequestService apiRequestService,
        IDbSessionProvider sessionProvider,
        IClientPaymentDao paymentDao,
        ISecurityManager securityManager
    )
    {
        _apiRequestService = apiRequestService;
        _sessionProvider = sessionProvider;
        _paymentDao = paymentDao;
        _securityManager = securityManager;
    }

    public async Task ExecuteAsync(DeleteRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var payment = await _paymentDao.GetById(request.ClientPaymentId);
        if (!await _securityManager.HasAccess(AccessLevel.Write, user, payment))
        {
            throw new HasNoAccessException();
        }

        await _sessionProvider.CurrentSession.DeleteAsync(payment);
    }
}
