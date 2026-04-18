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
        public async Task<StoredFileDto?> StorageUploadFileAsync(
            Guid entityId,
            StorageEntityType entityType,
            StoredFileType fileType,
            IBrowserFile file
        )
        {
            return await MultipartFormDataRequestAsync<StoredFileDto?>(
                ApiUrl.StorageUpload,
                new Dictionary<string, object>()
                {
                    // TaskId, commentId, etc.
                    { "EntityId", entityId },
                    { "EntityType", entityType },
                    { "FileType", fileType }
                },
                file
            );
        }
        
        public async Task StorageDeleteFileAsync(Guid fileId)
        {
            await PostAsync<object>(
                ApiUrl.StorageDelete,
                new DeleteRequest()
                {
                    Id = fileId
                }
            );
        }
        
        public async Task<GetListResponse?> StorageGetListAsync(
            Guid entityId,
            StorageEntityType entityType
        )
        {
            return await PostAsync<GetListResponse?>(
                ApiUrl.StorageList,
                new GetListRequest()
                {
                    EntityId = entityId,
                    EntityType = entityType
                }
            );
        }
    }
}
