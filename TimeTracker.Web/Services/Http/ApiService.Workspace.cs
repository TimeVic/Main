using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<PaginatedListDto<WorkspaceDto>> WorkspaceGetListAsync()
        {
            var response = await PostAuthorizedAsync<PaginatedListDto<WorkspaceDto>>(ApiUrl.WorkspaceList);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<WorkspaceDto> WorkspaceAddAsync(string name)
        {
            var response = await PostAuthorizedAsync<WorkspaceDto>(ApiUrl.WorkspaceAdd, new AddRequest()
            {
                Name = name
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<WorkspaceDto> WorkspaceUpdateAsync(long id, string name)
        {
            var response = await PostAuthorizedAsync<WorkspaceDto>(ApiUrl.WorkspaceUpdate, new UpdateRequest()
            {
                WorkspaceId = id,
                Name = name
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
