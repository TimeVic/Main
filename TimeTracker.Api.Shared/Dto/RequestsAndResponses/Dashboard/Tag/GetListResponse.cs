using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;

public class GetListResponse: PaginatedListDto<TagDto>
{
    public GetListResponse(
        ICollection<TagDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
