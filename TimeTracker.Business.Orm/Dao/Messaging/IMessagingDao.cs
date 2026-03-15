using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Messaging;

public interface IMessagingDao: IScopedDomainService
{
    Task<MessagingConnectionEntity> SetConnection(UserEntity user, string connectionId);

    Task DeleteConnection(UserEntity user, string connectionId);

    Task<MessagingMessageEntity> CreateMessage(MessagingChannelEntity channel, UserEntity user, string message);

    Task<ListDto<MessagingMessageEntity>> GetMessagesList(MessagingChannelEntity channel, int page, int pageSize = GlobalConstants.DefaultListPageSize);
    Task<MessagingCounterEntity?> GetMessageCount(MessagingChannelEntity channel, UserEntity user);
    Task<IList<MessagingCounterEntity>> GetMessageCounters(UserEntity user);
    Task<MessagingCounterEntity> IncreaseForUser(MessagingChannelEntity channel, UserEntity user);
    Task<IList<MessagingConnectionEntity>> GetConnectionsByUsers(IList<UserEntity> users);
    Task<(MessagingChannelEntity channel, bool isCreated)> GetOrCreateDirectChannel(
        WorkspaceEntity workspace,
        UserEntity sender,
        UserEntity receiver
    );
    Task<MessagingChannelEntity?> GetChannelBy(Guid id);

    Task<MessagingChannelEntity> CreateChannel(
        WorkspaceEntity workspace,
        UserEntity user,
        string slug
    );

    Task<List<MessagingChannelEntity>> GetChannelsList(WorkspaceEntity workspace, UserEntity user);
}
