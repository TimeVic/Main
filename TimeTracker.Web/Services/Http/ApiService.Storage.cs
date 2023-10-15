using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<StoredFileDto> StorageUploadFileAsync(
            long workspaceId,
            long entityId,
            StorageEntityType entityType,
            StoredFileType fileType,
            IBrowserFile file
        )
        {
            var response = await MultipartFormDataAuthorizedRequestAsync<StoredFileDto>(
                ApiUrl.StorageUpload,
                new Dictionary<string, object>()
                {
                    { "EntityId", entityId },
                    { "EntityType", entityType },
                    { "FileType", fileType },
                    { "WorkspaceId", workspaceId }
                },
                file
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task StorageDeleteFileAsync(long fileId)
        {
            await PostAuthorizedAsync<object>(
                ApiUrl.StorageDelete,
                new DeleteRequest()
                {
                    Id = fileId
                }
            );
        }
        
        public async Task<GetListResponse> StorageGetListAsync(
            long workspaceId,
            long entityId,
            StorageEntityType entityType
        )
        {
            var response = await PostAuthorizedAsync<GetListResponse>(
                ApiUrl.StorageList,
                new GetListRequest()
                {
                    WorkspaceId = workspaceId, 
                    EntityId = entityId,
                    EntityType = entityType
                }
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
