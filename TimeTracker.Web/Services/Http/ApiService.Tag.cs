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
            var response = await PostAuthorizedAsync<TagDto>(ApiUrl.TagAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<TagDto> TagUpdateAsync(UpdateRequest model)
        {
            var response = await PostAuthorizedAsync<TagDto>(ApiUrl.TagUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<GetListResponse> TagGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TagList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
