using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;

public class GetListResponse: PaginatedListDto<MemberPaymentDto>
{
    public GetListResponse(
        ICollection<MemberPaymentDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
