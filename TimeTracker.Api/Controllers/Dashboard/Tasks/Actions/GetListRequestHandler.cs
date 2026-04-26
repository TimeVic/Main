using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskListDao _taskListDao;
        private readonly ITaskDao _taskDao;

        public GetListRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _taskListDao = taskListDao;
            _taskDao = taskDao;
        }
    
        public async Task<GetListResponse> ExecuteAsync(GetListRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var taskList = await _taskListDao.GetById(request.TaskListId);
            RecordNotFoundException.ThrowIfNull(taskList);
            
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, taskList.Project))
            {
                throw new HasNoAccessException();
            }

            var filter = _mapper.Map<GetTasksFilterDto>(request.Filter);
            var taskLists = await _taskDao.GetList(taskList: taskList, filter: filter);
            var items = _mapper.Map<ICollection<TaskDto>>(taskLists.Items);
            var trackedDurationMap = await _taskDao.GetTrackedDurationByTaskIds(taskLists.Items.Select(item => item.Id).ToList());
            foreach (var item in items)
            {
                item.TrackedDuration = trackedDurationMap.TryGetValue(item.Id, out var trackedDuration)
                    ? trackedDuration
                    : TimeSpan.Zero;
            }

            return new GetListResponse(
                items,
                taskLists.TotalCount
            );
        }
    }
}
