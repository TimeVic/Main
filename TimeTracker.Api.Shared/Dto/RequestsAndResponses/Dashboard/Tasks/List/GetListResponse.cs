using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;

public class GetListResponse: PaginatedListDto<TaskListForListDto>
{
    public GetListResponse(
        ICollection<TaskListForListDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
