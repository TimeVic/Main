using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<GetIntegrationSettingsResponse?> WorkspaceIntegrationSettingsGetAsync(long workspaceId)
        {
            return await PostAsync<GetIntegrationSettingsResponse?>(
                ApiUrl.WorkspaceIntegrationSettingsGet,
                new GetIntegrationSettingsRequest() {
                    WorkspaceId = workspaceId
                }
            );
        }

        public async Task<WorkspaceSettingsRedmineDto?> WorkspaceSetRedmineIntegrationSettingsAsync(SetRedmineSettingsRequest settings)
        {
            return await PostAsync<WorkspaceSettingsRedmineDto?>(ApiUrl.WorkspaceIntegrationSettingsRedmineSet, settings);
        }

        public async Task<WorkspaceSettingsClickUpDto?> WorkspaceSetClickUpIntegrationSettingsAsync(SetClickUpSettingsRequest settings)
        {
            return await PostAsync<WorkspaceSettingsClickUpDto?>(ApiUrl.WorkspaceIntegrationSettingsClickUpSet, settings);
        }
        
        public async Task<WorkspaceSettingsJiraDto?> WorkspaceSetJiraIntegrationSettingsAsync(SetJiraSettingsRequest settings)
        {
            return await PostAsync<WorkspaceSettingsJiraDto?>(ApiUrl.WorkspaceIntegrationSettingsJiraSet, settings);
        }
    }
}
