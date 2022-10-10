using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Workspace.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, PaginatedListDto<WorkspaceDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;
        private readonly IUserDao _userDao;
        private readonly IWorkspaceAccessService _workspaceAccessService;

        public GetListRequestHandler(
            IMapper mapper,
            IRequestService requestService,
            IUserDao userDao,
            IWorkspaceAccessService workspaceAccessService
        )
        {
            _mapper = mapper;
            _requestService = requestService;
            _userDao = userDao;
            _workspaceAccessService = workspaceAccessService;
        }
    
        public async Task<PaginatedListDto<WorkspaceDto>> ExecuteAsync(GetListRequest request)
        {
            var userId = _requestService.GetUserIdFromJwt();
            var user = await _userDao.GetById(userId);
            var allWorkspaces = await _userDao.GetUsersWorkspaces(user);
            var responseList = _mapper.Map<ICollection<WorkspaceDto>>(allWorkspaces);
            foreach (var workspaceDto in responseList)
            {
                var workspace = allWorkspaces.First(item => item.Id == workspaceDto.Id);
                workspaceDto.CurrentUserAccess = await _workspaceAccessService.GetAccessTypeAsync(user, workspace);
            }
            return new PaginatedListDto<WorkspaceDto>(
                responseList,
                responseList.Count
            );
        }
    }
}
