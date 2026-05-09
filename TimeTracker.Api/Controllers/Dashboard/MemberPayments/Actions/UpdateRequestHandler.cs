using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.MemberPayments.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, MemberPaymentDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IMemberPaymentDao _paymentDao;
        private readonly IProjectDao _projectDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceDao _workspaceDao;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IMemberPaymentDao paymentDao,
            IProjectDao projectDao,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _paymentDao = paymentDao;
            _projectDao = projectDao;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
        }
    
        public async Task<MemberPaymentDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var payment = await _paymentDao.GetById(request.MemberPaymentId);
            RecordNotFoundException.ThrowIfNull(payment);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, payment))
            {
                throw new HasNoAccessException();
            }
            
            var project = await _projectDao.GetById(request.ProjectId);
            RecordNotFoundException.ThrowIfNull(project);
            if (
                project.Workspace.Id != payment.Member.Workspace.Id
                || !await _securityManager.HasAccess(AccessLevel.Read, user, project)
            )
            {
                throw new HasNoAccessException();
            }

            var member = payment.Member;
            if (request.MemberId != Guid.Empty && request.MemberId != payment.Member.Id)
            {
                if (!await _securityManager.HasAccess(AccessLevel.Write, user, payment.Member.Workspace))
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
                project,
                request.Amount,
                request.PaymentTime,
                request.Description
            );
            return _mapper.Map<MemberPaymentDto>(payment!);
        }
    }
}
