using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;

public class GetListResponse: PaginatedListDto<TaskCommentDto>
{
    public GetListResponse(
        ICollection<TaskCommentDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
