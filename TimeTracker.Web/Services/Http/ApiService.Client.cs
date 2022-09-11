using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<ClientDto> ClientAddAsync(AddRequest model)
        {
            var response = await PostAuthorizedAsync<ClientDto>(ApiUrl.ClientAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetListResponse> ClientGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.ClientList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
