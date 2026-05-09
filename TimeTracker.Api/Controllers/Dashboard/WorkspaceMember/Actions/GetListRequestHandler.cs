using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.WorkspaceMember.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly IWorkspaceDao _workspaceDao;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            IWorkspaceDao workspaceDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _workspaceDao = workspaceDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var userId = _apiRequestService.GetCurrentUserId();
            var user = await _userDao.GetById(userId);
            var workspace = await _userDao.GetUsersWorkspace(user!, _apiRequestService.GetCurrentWorkspaceId());
            if (!await _securityManager.HasAccess(AccessLevel.Read, user!, workspace))
            {
                throw new HasNoAccessException();
            }

            var listDto = await _workspaceDao.GetMembersAsync(workspace!, request.Page);
            return new GetListResponse(
                _mapper.Map<ICollection<WorkspaceMemberDto>>(listDto.Items),
                listDto.TotalCount
            );
        }
    }
}
