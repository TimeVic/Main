using Api.Requests.Abstractions;
using AutoMapper;
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
    public class AddRequestHandler : IAsyncRequestHandler<AddRequest, MemberPaymentDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IMemberPaymentDao _paymentDao;
        private readonly IProjectDao _projectDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceDao _workspaceDao;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public AddRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IMemberPaymentDao paymentDao,
            IProjectDao projectDao,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _paymentDao = paymentDao;
            _projectDao = projectDao;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<MemberPaymentDto> ExecuteAsync(AddRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var project = await _projectDao.GetById(request.ProjectId);
            RecordNotFoundException.ThrowIfNull(project);
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            if (
                workspace == null 
                || workspace.Mode != WorkspaceMode.Team
                || project.Client.Workspace.Id != workspace.Id
                || !await _securityManager.HasAccess(AccessLevel.Read, user, workspace)
                || !await _securityManager.HasAccess(AccessLevel.Read, user, project)
            )
            {
                throw new HasNoAccessException();
            }

            var currentMember = _workspaceAccessService.GetMemberAsync(user, workspace);
            if (currentMember == null)
            {
                throw new HasNoAccessException();
            }

            var member = currentMember;
            if (request.MemberId != Guid.Empty && request.MemberId != currentMember.Id)
            {
                if (!await _securityManager.HasAccess(AccessLevel.Write, user, workspace))
                {
                    throw new HasNoAccessException();
                }

                member = await _workspaceDao.GetMemberAsync(request.MemberId);
                if (member == null || member.Workspace.Id != workspace.Id)
                {
                    throw new HasNoAccessException();
                }
            }

            var payment = await _paymentDao.CreateAsync(
                member,
                project,
                request.Amount,
                request.PaymentTime,
                request.Description
            );
            return _mapper.Map<MemberPaymentDto>(payment);
        }
    }
}
