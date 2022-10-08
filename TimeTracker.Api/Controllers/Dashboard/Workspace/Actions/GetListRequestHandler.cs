using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, PaginatedListDto<WorkspaceDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;

        public GetListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
        }
    
        public async Task<PaginatedListDto<WorkspaceDto>> ExecuteAsync(GetListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var allWorkspaces = await _userDao.GetUsersWorkspaces(user);
            return new PaginatedListDto<WorkspaceDto>(
                _mapper.Map<ICollection<WorkspaceDto>>(allWorkspaces),
                user.Workspaces.Count
            );
        }
    }
}
