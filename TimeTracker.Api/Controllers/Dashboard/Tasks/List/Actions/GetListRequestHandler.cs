using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.List.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest listRequest)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var workspace = await _userDao.GetUsersWorkspace(user, listRequest.WorkspaceId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, workspace))
            {
                throw new HasNoAccessException();
            }

            var taskLists = await _taskListDao.GetList(workspace);
            return new GetListResponse(
                _mapper.Map<ICollection<TaskListDto>>(taskLists.Items),
                taskLists.TotalCount
            );
        }
    }
}
