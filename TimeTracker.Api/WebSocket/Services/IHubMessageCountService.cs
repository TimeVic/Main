using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.WebSocket.Services;

public interface IHubMessageCountService: IScopedDomainService
{
    Task IncreaseForUsers(MessagingChannelEntity channel, IList<UserEntity> users);

    Task IncreaseForUser(MessagingChannelEntity channel, UserEntity user);
    
    Task ResetForUser(MessagingChannelEntity channel, UserEntity user);
}
