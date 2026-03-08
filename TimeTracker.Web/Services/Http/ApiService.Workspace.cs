using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
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
        
        public async Task<WorkspaceDto?> WorkspaceUpdateAsync(Guid id, string name)
        {
            return await PostAsync<WorkspaceDto>(ApiUrl.WorkspaceUpdate, new UpdateRequest()
            {
                WorkspaceId = id,
                Name = name
            });
        }
    }
}
