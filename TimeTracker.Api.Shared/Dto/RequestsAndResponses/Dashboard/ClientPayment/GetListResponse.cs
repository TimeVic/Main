using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;

public class GetListResponse: PaginatedListDto<ClientPaymentDto>
{
    public GetListResponse(
        ICollection<ClientPaymentDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
