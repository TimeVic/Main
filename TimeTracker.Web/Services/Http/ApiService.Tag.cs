using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TagDto?> TagAddAsync(AddRequest model)
        {
            return await PostAsync<TagDto?>(ApiUrl.TagAdd, model);
        }

        public async Task<TagDto?> TagUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<TagDto?>(ApiUrl.TagUpdate, model);
        }
        
        public async Task TagDeleteAsync(Guid tagId)
        {
            await PostAsync<object>(ApiUrl.TagDelete, new DeleteRequest()
            {
                TagId = tagId
            });
        }

        public async Task<GetListResponse?> TagGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.TagList, model);
        }
    }
}
