using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TagDto> TagAddAsync(AddRequest model)
        {
            var response = await PostAsync<TagDto>(ApiUrl.TagAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<TagDto> TagUpdateAsync(UpdateRequest model)
        {
            var response = await PostAsync<TagDto>(ApiUrl.TagUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task TagDeleteAsync(long tagId)
        {
            await PostAsync<object>(ApiUrl.TagDelete, new DeleteRequest()
            {
                TagId = tagId
            });
        }

        public async Task<GetListResponse> TagGetListAsync(GetListRequest model)
        {
            var response = await PostAsync<GetListResponse>(ApiUrl.TagList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
