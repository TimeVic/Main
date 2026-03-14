using Domain.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.WebSocket.Services.Mappers;

public interface IHubMapperService: IScopedDomainService
{
    List<MessagingMessageDto> MapToMessageDto(List<MessagingMessageEntity> messages, UserEntity currentUser);

    MessagingMessageDto MapToMessageDto(MessagingMessageEntity message, UserEntity currentUser);
}
