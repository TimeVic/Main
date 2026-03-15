using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.WebSocket.Services;

public interface IHubMessagingService: IScopedDomainService
{
    Task<MessagingMessageEntity> SendMessage(
        WorkspaceEntity workspace,
        UserEntity sender,
        string messageText,
        UserEntity? receiver = null,
        MessagingChannelEntity? channel = null
    );

    Task<MessagingChannelEntity> CreateChannel(
        WorkspaceEntity workspace,
        UserEntity user,
        string slug,
        List<UserEntity> members
    );

    Task InitChannels(WorkspaceEntity workspace, UserEntity user);
}
