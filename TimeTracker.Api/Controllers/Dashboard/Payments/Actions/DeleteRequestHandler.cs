using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Payments.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly IPaymentDao _paymentDao;
        private readonly ISecurityManager _securityManager;

        public DeleteRequestHandler(
            IRequestService requestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            IPaymentDao paymentDao,
            ISecurityManager securityManager
        )
        {
            _requestService = requestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _paymentDao = paymentDao;
            _securityManager = securityManager;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            
            var payment = await _paymentDao.GetById(request.PaymentId);
            if (payment == null || !await _securityManager.HasAccess(AccessLevel.Write, user, payment))
            {
                throw new HasNoAccessException();
            }
            await _sessionProvider.CurrentSession.DeleteAsync(payment);
        }
    }
}
