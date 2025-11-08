using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<PaymentDto?> PaymentAddAsync(AddRequest model)
        {
            return await PostAsync<PaymentDto?>(ApiUrl.PaymentAdd, model);
        }
        
        public async Task<PaymentDto?> PaymentUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<PaymentDto?>(ApiUrl.PaymentUpdate, model);
        }
        
        public async Task PaymentDeleteAsync(long paymentId)
        {
            await PostAsync<object>(ApiUrl.PaymentDelete, new DeleteRequest()
            {
                PaymentId = paymentId
            });
        }
        
        public async Task<GetListResponse?> PaymentGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.PaymentList, model);
        }
    }
}
