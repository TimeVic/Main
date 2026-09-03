using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.SubTasks.Actions
{
    public class UpdateRequestHandler : IAsyncRequestHandler<UpdateRequest, TaskSubTaskDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly ITaskSubTaskDao _taskSubTaskDao;
        private readonly ISecurityManager _securityManager;

        public UpdateRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            ITaskSubTaskDao taskSubTaskDao,
            ISecurityManager securityManager
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _taskSubTaskDao = taskSubTaskDao;
            _securityManager = securityManager;
        }

        public async Task<TaskSubTaskDto> ExecuteAsync(UpdateRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var subTask = await _taskSubTaskDao.GetById(request.SubTaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Write, user, subTask))
            {
                throw new HasNoAccessException();
            }

            subTask = await _taskSubTaskDao.UpdateAsync(subTask!, request.Title, request.IsCompleted);
            return _mapper.Map<TaskSubTaskDto>(subTask);
        }
    }
}
