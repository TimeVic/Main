using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<GetIntegrationSettingsResponse> WorkspaceIntegrationSettingsGetAsync()
        {
            var response = await PostAuthorizedAsync<GetIntegrationSettingsResponse>(ApiUrl.WorkspaceIntegrationSettingsGet);
            if (response == null)
            {
                throw new ServerErrorException();
            }
            return response;
        }

        public async Task<WorkspaceSettingsRedmineDto> WorkspaceSetRedmineIntegrationSettingsAsync(WorkspaceSettingsRedmineDto settings)
        {
            var response = await PostAuthorizedAsync<WorkspaceSettingsRedmineDto>(ApiUrl.WorkspaceIntegrationSettingsRedmineSet);
            if (response == null)
            {
                throw new ServerErrorException();
            }
            return response;
        }

        public async Task<WorkspaceSettingsClickUpDto> WorkspaceSetClickUpIntegrationSettingsAsync(WorkspaceSettingsClickUpDto settings)
        {
            var response = await PostAuthorizedAsync<WorkspaceSettingsClickUpDto>(ApiUrl.WorkspaceIntegrationSettingsClickUpSet);
            if (response == null)
            {
                throw new ServerErrorException();
            }
            return response;
        }
    }
}
