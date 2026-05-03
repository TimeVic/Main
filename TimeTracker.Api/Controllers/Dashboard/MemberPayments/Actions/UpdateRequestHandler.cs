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
        private readonly IWorkspaceDao _workspaceDao;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IDbSessionProvider sessionProvider,
            IMemberPaymentDao paymentDao,
            IClientDao clientDao,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _sessionProvider = sessionProvider;
            _paymentDao = paymentDao;
            _clientDao = clientDao;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
            _workspaceAccessService = workspaceAccessService;
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

            var member = payment!.Member;
            if (request.MemberId != Guid.Empty && request.MemberId != payment.Member.Id)
            {
                var accessType = await _workspaceAccessService.GetAccessTypeAsync(user, payment.Member.Workspace);
                if (accessType is not (MembershipAccessType.Owner or MembershipAccessType.Manager))
                {
                    throw new HasNoAccessException();
                }

                member = await _workspaceDao.GetMemberAsync(request.MemberId);
                if (member == null || member.Workspace.Id != payment.Member.Workspace.Id)
                {
                    throw new HasNoAccessException();
                }
            }

            payment = await _paymentDao.UpdateMemberPaymentAsync(
                request.MemberPaymentId,
                member,
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
