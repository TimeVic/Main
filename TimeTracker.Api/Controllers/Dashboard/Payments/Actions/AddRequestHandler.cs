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
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, PaymentDto>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly IPaymentDao _paymentDao;
        private readonly IClientDao _clientDao;
        private readonly ISecurityManager _securityManager;

        public AddRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            IPaymentDao paymentDao,
            IClientDao clientDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _paymentDao = paymentDao;
            _clientDao = clientDao;
            _securityManager = securityManager;
        }
    
        public async Task<PaymentDto> ExecuteAsync(AddRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var client = await _clientDao.GetById(request.ClientId);
            if (client == null || !await _securityManager.HasAccess(AccessLevel.Write, user, client))
            {
                throw new HasNoAccessException();
            }

            var payment = await _paymentDao.CreateAsync(
                client,
                request.Amount,
                request.PaymentTime,
                request.ProjectId,
                request.Description
            );
            await _sessionProvider.PerformCommitAsync();
            
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}
