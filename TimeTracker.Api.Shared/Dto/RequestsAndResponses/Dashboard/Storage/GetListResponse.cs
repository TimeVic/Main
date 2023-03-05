using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;

public class GetListResponse: PaginatedListDto<StoredFileDto>
{
    public GetListResponse(
        ICollection<StoredFileDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
