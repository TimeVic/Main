using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Payments.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, PaymentDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly IPaymentDao _paymentDao;
        private readonly IClientDao _clientDao;
        private readonly ISecurityManager _securityManager;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            IPaymentDao paymentDao,
            IClientDao clientDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _paymentDao = paymentDao;
            _clientDao = clientDao;
            _securityManager = securityManager;
        }
    
        public async Task<PaymentDto> ExecuteAsync(UpdateRequest request)
        {
            var userId = _apiRequestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var payment = await _paymentDao.GetById(request.PaymentId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, payment))
            {
                throw new HasNoAccessException();
            }
            
            var client = await _clientDao.GetById(request.ClientId);
            if (
                client == null
                || !await _securityManager.HasAccess(AccessLevel.Read, user, client)
            )
            {
                throw new HasNoAccessException();
            }

            payment = await _paymentDao.UpdatePaymentAsync(
                request.PaymentId,
                client,
                request.Amount,
                request.PaymentTime,
                request.ProjectId,
                request.Description
            );
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}
