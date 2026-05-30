using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;

namespace TimeTracker.Client.Core.Services.Http;

public partial class ApiService
{
    public async Task<ClientPaymentDto?> ClientPaymentAddAsync(AddRequest model)
    {
        return await PostAsync<ClientPaymentDto?>(ApiUrl.ClientPaymentAdd, model);
    }

    public async Task<ClientPaymentDto?> ClientPaymentUpdateAsync(UpdateRequest model)
    {
        return await PostAsync<ClientPaymentDto?>(ApiUrl.ClientPaymentUpdate, model);
    }

    public async Task ClientPaymentDeleteAsync(Guid paymentId)
    {
        await PostAsync<object>(ApiUrl.ClientPaymentDelete, new DeleteRequest
        {
            ClientPaymentId = paymentId
        });
    }

    public async Task<GetListResponse?> ClientPaymentGetListAsync(GetListRequest model)
    {
        return await PostAsync<GetListResponse?>(ApiUrl.ClientPaymentList, model);
    }
}
