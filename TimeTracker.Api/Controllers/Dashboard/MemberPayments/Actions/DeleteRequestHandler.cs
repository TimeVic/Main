using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.MemberPayments.Actions
{
    public class DeleteRequestHandler : IAsyncRequestHandler<DeleteRequest>
    {
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly IMemberPaymentDao _paymentDao;
        private readonly ISecurityManager _securityManager;

        public DeleteRequestHandler(
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            IMemberPaymentDao paymentDao,
            ISecurityManager securityManager
        )
        {
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _paymentDao = paymentDao;
            _securityManager = securityManager;
        }
    
        public async Task ExecuteAsync(DeleteRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var payment = await _paymentDao.GetById(request.MemberPaymentId);
            RecordNotFoundException.ThrowIfNull(payment);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, payment))
            {
                throw new HasNoAccessException();
            }
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, payment.Member.Workspace))
            {
                throw new HasNoAccessException();
            }
            await _sessionProvider.CurrentSession.DeleteAsync(payment);
        }
    }
}
