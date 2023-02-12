using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Exceptions;

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
    }
}
