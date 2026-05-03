using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<MemberPaymentDto?> MemberPaymentAddAsync(AddRequest model)
        {
            return await PostAsync<MemberPaymentDto?>(ApiUrl.MemberPaymentAdd, model);
        }
        
        public async Task<MemberPaymentDto?> MemberPaymentUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<MemberPaymentDto?>(ApiUrl.MemberPaymentUpdate, model);
        }
        
        public async Task MemberPaymentDeleteAsync(Guid paymentId)
        {
            await PostAsync<object>(ApiUrl.MemberPaymentDelete, new DeleteRequest()
            {
                MemberPaymentId = paymentId
            });
        }
        
        public async Task<GetListResponse?> MemberPaymentGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse?>(ApiUrl.MemberPaymentList, model);
        }
    }
}
