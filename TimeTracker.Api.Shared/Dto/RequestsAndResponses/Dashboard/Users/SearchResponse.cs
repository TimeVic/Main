using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class SearchResponse : PaginatedListDto<UserDto>
{
    public SearchResponse(
        ICollection<UserDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
