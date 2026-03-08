using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<ProjectDto?> ProjectAddAsync(AddRequest model)
        {
            return await PostAsync<ProjectDto?>(ApiUrl.ProjectAdd, model);
        }

        public async Task<ProjectDto?> ProjectUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<ProjectDto?>(ApiUrl.ProjectUpdate, model);
        }

        public async Task<GetListResponse?> ProjectGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.ProjectList, model);
        }
        
        public async Task ProjectDeleteAsync(Guid projectId)
        {
            await PostAsync<ProjectDto>(ApiUrl.ProjectDelete, new DeleteRequest()
            {
                ProjectId = projectId
            });
        }
    }
}
