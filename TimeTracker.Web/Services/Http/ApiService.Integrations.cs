using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<GetIntegrationSettingsResponse> WorkspaceIntegrationSettingsGetAsync(long workspaceId)
        {
            var response = await PostAuthorizedAsync<GetIntegrationSettingsResponse>(
                ApiUrl.WorkspaceIntegrationSettingsGet,
                new GetIntegrationSettingsRequest() {
                    WorkspaceId = workspaceId
                }
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }
            return response;
        }

        public async Task<WorkspaceSettingsRedmineDto> WorkspaceSetRedmineIntegrationSettingsAsync(SetRedmineSettingsRequest settings)
        {
            var response = await PostAuthorizedAsync<WorkspaceSettingsRedmineDto>(
                ApiUrl.WorkspaceIntegrationSettingsRedmineSet,
                settings
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }
            return response;
        }

        public async Task<WorkspaceSettingsClickUpDto> WorkspaceSetClickUpIntegrationSettingsAsync(SetClickUpSettingsRequest settings)
        {
            var response = await PostAuthorizedAsync<WorkspaceSettingsClickUpDto>(
                ApiUrl.WorkspaceIntegrationSettingsClickUpSet,
                settings
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }
            return response;
        }
    }
}
