using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;

public class GetListResponse: PaginatedListDto<ClientDto>
{
    public GetListResponse(
        ICollection<ClientDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
