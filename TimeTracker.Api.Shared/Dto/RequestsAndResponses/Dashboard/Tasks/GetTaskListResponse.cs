using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

public class GetTaskListResponse: PaginatedListDto<TaskListDto>
{
    public GetTaskListResponse(
        ICollection<TaskListDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
