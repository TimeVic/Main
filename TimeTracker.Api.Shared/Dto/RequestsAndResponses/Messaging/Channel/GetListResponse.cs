using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;

public class GetListResponse: PaginatedListDto<MessagingChannelDto>
{
    public GetListResponse(
        ICollection<MessagingChannelDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
