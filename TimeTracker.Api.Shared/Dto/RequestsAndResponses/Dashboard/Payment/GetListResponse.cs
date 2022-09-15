using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;

public class GetListResponse: PaginatedListDto<PaymentDto>
{
    public GetListResponse(
        ICollection<PaymentDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
