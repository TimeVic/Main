using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;

namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<TaskSubTaskDto?> TaskSubTaskAddAsync(AddRequest model)
        {
            return await PostAsync<TaskSubTaskDto?>(ApiUrl.TaskSubTaskAdd, model);
        }

        public async Task<TaskSubTaskDto?> TaskSubTaskUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<TaskSubTaskDto?>(ApiUrl.TaskSubTaskUpdate, model);
        }

        public async Task TaskSubTaskDeleteAsync(Guid subTaskId)
        {
            await PostAsync(ApiUrl.TaskSubTaskDelete, new DeleteRequest
            {
                SubTaskId = subTaskId
            });
        }
    }
}
