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
        public async Task<TaskDto> TasksAddAsync(AddRequest model)
        {
            var response = await PostAuthorizedAsync<TaskDto>(ApiUrl.TasksAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<TaskDto> TasksUpdateAsync(UpdateRequest model)
        {
            var response = await PostAuthorizedAsync<TaskDto>(ApiUrl.TasksUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<GetListResponse> TasksGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TasksList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetListResponse> TasksGetForCalendarAsync(GetForCalendarRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TasksListForCalendar, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetListResponse> TasksGetMyListAsync(
            long workspaceId,
            ICollection<TaskStatus>? taskStatuses = null,
            string? searchString = null
        )
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TasksMyList, new GetMyListRequest
            {
                WorkspaceId = workspaceId,
                Statuses = taskStatuses,
                SearchString = searchString
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<TaskDto?> TasksGetAsync(long workspaceId, long taskId)
        {
            var response = await PostAuthorizedAsync<TaskDto?>(ApiUrl.TasksGetOne, new GetOneRequest()
            {
                WorkspaceId = workspaceId,
                TaskId = taskId
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
