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
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, MemberPaymentDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IDbSessionProvider _sessionProvider;
        private readonly IMemberPaymentDao _paymentDao;
        private readonly IClientDao _clientDao;
        private readonly ISecurityManager _securityManager;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            IMemberPaymentDao paymentDao,
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
    
        public async Task<MemberPaymentDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var payment = await _paymentDao.GetById(request.MemberPaymentId);
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

            payment = await _paymentDao.UpdateMemberPaymentAsync(
                request.MemberPaymentId,
                client,
                request.Amount,
                request.PaymentTime,
                request.ProjectId,
                request.Description
            );
            return _mapper.Map<MemberPaymentDto>(payment);
        }
    }
}
