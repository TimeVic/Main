using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

public class GetListResponse: PaginatedListDto<TaskDto>
{
    public TaskListDto? TaskList { get; set; }

    public GetListResponse(
        ICollection<TaskDto> responseList,
        int totalItems,
        TaskListDto? taskList = null
    ) : base(responseList, totalItems)
    {
        TaskList = taskList;
    }
}
