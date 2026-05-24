using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<ListResponse<CurrencyDto>> ListCurrenciesGetAll()
        {
            return (await PostAsync<ListResponse<CurrencyDto>>(ApiUrl.ListCurrencyList))!;
        }
    }
}
