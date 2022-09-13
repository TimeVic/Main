using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<ProjectDto> ProjectAddAsync(AddRequest model)
        {
            var response = await PostAuthorizedAsync<ProjectDto>(ApiUrl.ProjectAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<ProjectDto> ProjectUpdateAsync(UpdateRequest model)
        {
            var response = await PostAuthorizedAsync<ProjectDto>(ApiUrl.ProjectUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<GetListResponse> ProjectGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.ProjectList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
