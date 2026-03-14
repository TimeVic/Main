using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Messaging;

public partial class MessagingDao
{
    public async Task<MessagingChannelEntity> GetOrCreateDirectChannel(
        WorkspaceEntity workspace,
        UserEntity sender,
        UserEntity receiver
    )
    {
        var channel = await Session.Query<MessagingChannelEntity>()
            .Where(item => item.Type == MessagingChannelType.Direct)
            .Where(
                item => item.CreatedBy == sender && item.User == receiver
                    || item.CreatedBy == receiver && item.User == sender
            )
            .FirstOrDefaultAsync();
        if (channel is not null)
        {
            return channel;
        }

        channel = new MessagingChannelEntity
        {
            Type = MessagingChannelType.Direct,
            Workspace = workspace,
            User = receiver,
            CreatedBy = sender,
            CreatedAt = DateTime.UtcNow
        };
        await Session.SaveAsync(channel);
        return channel;
    }
    
    public async Task<MessagingChannelEntity?> GetChannelBy(Guid id)
    {
        var channel = await Session.Query<MessagingChannelEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
        return channel;
    }
}
