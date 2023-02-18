using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TimeTracker.Api.Shared.Dto.Entity.StoredFileDto> StorageUploadFileAsync(
            long entityId,
            StorageEntityType entityType,
            StoredFileType fileType,
            IBrowserFile file
        )
        {
            var response = await MultipartFormDataRequestAsync<StoredFileDto>(
                ApiUrl.StorageUpload,
                new Dictionary<string, object>()
                {
                    { "EntityId", entityId },
                    { "EntityType", entityType },
                    { "FileType", fileType },
                },
                file
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
