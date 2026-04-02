using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<ClientDto?> ClientAddAsync(AddRequest model)
        {
            return await PostAsync<ClientDto?>(ApiUrl.ClientAdd, model);
        }
        
        public async Task<ClientDto> ClientUpdateAsync(UpdateRequest model)
        {
            return (await PostAsync<ClientDto>(ApiUrl.ClientUpdate, model))!;
        }
        
        public async Task<GetListResponse?> ClientGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.ClientList, model);
        }
    }
}
