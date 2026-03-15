using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;

public class GetListResponse: PaginatedListDto<MessagingMessageDto>
{
    public GetListResponse(
        ICollection<MessagingMessageDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
