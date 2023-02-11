using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;

public class GetListResponse: PaginatedListDto<TaskListDto>
{
    public GetListResponse(
        ICollection<TaskListDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
