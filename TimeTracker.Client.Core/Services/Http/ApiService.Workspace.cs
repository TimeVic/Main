using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Common.Dto;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<PaginatedListDto<WorkspaceDto>?> WorkspaceGetListAsync()
        {
            return await PostAsync<PaginatedListDto<WorkspaceDto>>(ApiUrl.WorkspaceList);
        }
        
        public async Task<WorkspaceDto?> WorkspaceAddAsync(string name)
        {
            return await PostAsync<WorkspaceDto>(ApiUrl.WorkspaceAdd, new AddRequest()
            {
                Name = name
            });
        }
        
        public async Task<WorkspaceDto?> WorkspaceUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<WorkspaceDto>(ApiUrl.WorkspaceUpdate, model);
        }

        public async Task<WorkspaceDto?> WorkspaceSetModeAsync(TimeTracker.Business.Common.Constants.WorkspaceMode mode)
        {
            return await PostAsync<WorkspaceDto>(ApiUrl.WorkspaceSetMode, new SetModeRequest
            {
                Mode = mode
            });
        }

        public async Task WorkspaceDeleteAsync(DeleteRequest request)
        {
            await PostAsync<object>(ApiUrl.WorkspaceDelete, request);
        }

        public async Task<ImportResponse?> WorkspaceTimeEntryImportAsync(
            TimeEntryImportSourceType sourceType,
            bool isBillable,
            decimal? hourlyRate,
            IBrowserFile file
        )
        {
            var data = new Dictionary<string, object>()
            {
                { "SourceType", (int)sourceType },
                { "IsBillable", isBillable }
            };
            if (hourlyRate.HasValue)
            {
                data.Add("HourlyRate", hourlyRate.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            return await MultipartFormDataRequestAsync<ImportResponse?>(
                ApiUrl.WorkspaceTimeEntryImport,
                data,
                file
            );
        }
    }
}
