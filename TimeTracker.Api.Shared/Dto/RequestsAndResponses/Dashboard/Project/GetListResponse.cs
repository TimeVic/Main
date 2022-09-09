using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;

public class GetListResponse: PaginatedListDto<ProjectDto>
{
    public GetListResponse(
        ICollection<ProjectDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
