using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

public class GetListResponse: PaginatedListDto<TaskDto>
{
    public GetListResponse(
        ICollection<TaskDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
