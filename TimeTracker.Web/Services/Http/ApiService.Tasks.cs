using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Exceptions;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TaskDto?> TasksAddAsync(AddRequest model)
        {
            return await PostAsync<TaskDto>(ApiUrl.TasksAdd, model);
        }

        public async Task<TaskDto?> TasksUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<TaskDto>(ApiUrl.TasksUpdate, model);
        }
        
        public async Task TasksUpdatePositionsAsync(UpdatePositionsRequest request)
        {
            await PostAsync<TaskDto>(ApiUrl.TasksUpdatePositions, request);
        }

        public async Task<GetListResponse?> TasksGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse>(ApiUrl.TasksList, model);
        }
        
        public async Task<GetListResponse?> TasksGetForCalendarAsync(GetForCalendarRequest model)
        {
            return await PostAsync<GetListResponse>(ApiUrl.TasksListForCalendar, model);
        }
        
        public async Task<GetListResponse?> TasksGetMyListAsync(
            Guid workspaceId,
            ICollection<TaskStatus>? taskStatuses = null,
            string? searchString = null
        )
        {
            return await PostAsync<GetListResponse?>(ApiUrl.TasksMyList, new GetMyListRequest
            {
                WorkspaceId = workspaceId,
                Statuses = taskStatuses,
                SearchString = searchString
            });
        }
        
        public async Task<GetListResponse?> TasksGetOverdueListAsync(
            Guid workspaceId,
            string? searchString = null
        )
        {
            return await PostAsync<GetListResponse>(ApiUrl.TasksMyList, new GetMyListRequest
            {
                WorkspaceId = workspaceId,
                Statuses = new List<TaskStatus>()
                {
                    TaskStatus.Backlog,
                    TaskStatus.ToDo,
                    TaskStatus.InProgress,
                },
                SearchString = searchString,
                EndTime = DateTime.UtcNow.AddMonths(12)
            });
        }
        
        public async Task<TaskDto?> TasksGetAsync(Guid taskId)
        {
            return await PostAsync<TaskDto?>(ApiUrl.TasksGetOne, new GetOneRequest()
            {
                TaskId = taskId
            });
        }
    }
}
