using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<PaymentDto> PaymentAddAsync(AddRequest model)
        {
            var response = await PostAuthorizedAsync<PaymentDto>(ApiUrl.PaymentAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<PaymentDto> PaymentUpdateAsync(UpdateRequest model)
        {
            var response = await PostAuthorizedAsync<PaymentDto>(ApiUrl.PaymentUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task PaymentDeleteAsync(DeleteRequest model)
        {
            await PostAuthorizedAsync<object>(ApiUrl.PaymentDelete, model);
        }
        
        public async Task<GetListResponse> PaymentGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.PaymentList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
