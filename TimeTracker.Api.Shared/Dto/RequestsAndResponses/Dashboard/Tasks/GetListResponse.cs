using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Dto;

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
