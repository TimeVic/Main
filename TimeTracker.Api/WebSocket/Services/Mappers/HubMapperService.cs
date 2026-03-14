using AutoMapper;
using TimeTracker.Api.Shared.Constants.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.WebSocket.Services.Mappers;

public class HubMapperService: IHubMapperService
{
    private readonly IMapper _mapper;

    public HubMapperService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public MessagingMessageDto MapToMessageDto(MessagingMessageEntity message, UserEntity currentUser)
    {
        return MapToMessageDto([message], currentUser).First();
    }

    public List<MessagingMessageDto> MapToMessageDto(List<MessagingMessageEntity> messages, UserEntity currentUser)
    {
        return messages.Select(message =>
        {
            var messageDto = _mapper.Map<MessagingMessageDto>(message);
            ArgumentNullException.ThrowIfNull(currentUser);
            ArgumentNullException.ThrowIfNull(message.Channel);

            if (message.CreatedBy == currentUser)
                messageDto.Direction = HubMessageDirectionEnum.Outgoing;
            else
                messageDto.Direction = HubMessageDirectionEnum.Incoming;

            return messageDto;
        }).ToList();
    }
}
