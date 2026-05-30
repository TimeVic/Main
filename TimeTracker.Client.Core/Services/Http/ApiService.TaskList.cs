using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<TaskListDto?> TaskListAddAsync(AddRequest model)
        {
            return await PostAsync<TaskListDto?>(ApiUrl.TaskListAdd, model);
        }

        public async Task<TaskListDto?> TaskListUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<TaskListDto?>(ApiUrl.TaskListUpdate, model);
        }

        public async Task<GetListResponse?> TaskListGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.TaskListList, model);
        }
        
        public async Task TaskListArchiveAsync(Guid taskListId)
        {
            await PostAsync<TaskListDto>(ApiUrl.TaskListArchive, new ArchiveTaskListRequest()
            {
                TaskListId = taskListId
            });
        }
    }
}
