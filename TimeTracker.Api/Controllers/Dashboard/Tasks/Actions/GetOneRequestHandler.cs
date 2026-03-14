using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Api.Controllers.Dashboard.Tasks.Actions
{
    public class GetOneRequestHandler : IAsyncRequestHandler<GetOneRequest, TaskFullDto>
    {
        private readonly IMapper _mapper;
        private readonly IApiRequestService _apiRequestService;
        private readonly IUserDao _userDao;
        private readonly ISecurityManager _securityManager;
        private readonly ITaskDao _taskDao;

        public GetOneRequestHandler(
            IMapper mapper,
            IApiRequestService apiRequestService,
            IUserDao userDao,
            ISecurityManager securityManager,
            ITaskListDao taskListDao,
            ITaskDao taskDao,
            IDbSessionProvider dbSessionProvider
        )
        {
            _mapper = mapper;
            _apiRequestService = apiRequestService;
            _userDao = userDao;
            _securityManager = securityManager;
            _taskDao = taskDao;
        }
    
        public async Task<TaskFullDto> ExecuteAsync(GetOneRequest request)
        {
            var user = await _apiRequestService.GetCurrentUser();
            var task = await _taskDao.GetById(request.TaskId);
            if (!await _securityManager.HasAccess(AccessLevel.Read, user, task))
            {
                throw new HasNoAccessException();
            }
            return _mapper.Map<TaskFullDto>(task);
        }
    }
}
