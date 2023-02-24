using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TaskListDto> TaskListAddAsync(AddRequest model)
        {
            var response = await PostAuthorizedAsync<TaskListDto>(ApiUrl.TaskListAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<TaskListDto> TaskListUpdateAsync(UpdateRequest model)
        {
            var response = await PostAuthorizedAsync<TaskListDto>(ApiUrl.TaskListUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<GetListResponse> TaskListGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TaskListList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task TaskListArchiveAsync(long taskListId)
        {
            await PostAuthorizedAsync<TaskListDto>(ApiUrl.TaskListArchive, new ArchiveTaskListRequest()
            {
                TaskListId = taskListId
            });
        }
    }
}
