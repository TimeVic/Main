using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;

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
