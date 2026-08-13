using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.MemberPayments.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly IMemberPaymentDao _paymentDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceDao _workspaceDao;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            IMemberPaymentDao paymentDao,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _paymentDao = paymentDao;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, _apiRequestService.GetCurrentWorkspaceId());
            RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");
            if (workspace.Mode != WorkspaceMode.Team || !await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var hasWriteAccessToWorkspace = await _securityManager.HasAccess(AccessLevel.Write, user, workspace);
            if (!hasWriteAccessToWorkspace)
            {
                if (request.MemberId != Guid.Empty)
                {
                    throw new HasNoAccessException();
                }

                var userListDto = await _paymentDao.GetListAsync(workspace, user, request.Page);
                return new GetListResponse(
                    _mapper.Map<ICollection<MemberPaymentDto>>(userListDto.Items),
                    userListDto.TotalCount
                );
            }

            var listDto = await GetListForWorkspaceUserAsync(request, workspace);

            return new GetListResponse(
                _mapper.Map<ICollection<MemberPaymentDto>>(listDto.Items),
                listDto.TotalCount
            );
        }

        private async Task<ListDto<MemberPaymentEntity>> GetListForWorkspaceUserAsync(
            GetListRequest request,
            WorkspaceEntity workspace
        )
        {
            if (request.MemberId == Guid.Empty)
            {
                return await _paymentDao.GetListAsync(workspace, request.Page);
            }

            var member = await _workspaceDao.GetMemberAsync(request.MemberId);
            if (member == null || member.Workspace.Id != workspace.Id)
            {
                throw new HasNoAccessException();
            }

            return await _paymentDao.GetListAsync(member, request.Page);
        }
    }
}
