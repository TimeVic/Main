using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Messaging;

public partial class MessagingDao
{
    public async Task<(MessagingChannelEntity channel, bool isCreated)> GetOrCreateDirectChannel(
        WorkspaceEntity workspace,
        UserEntity sender,
        UserEntity receiver
    )
    {
        var channel = await Session.Query<MessagingChannelEntity>()
            .Where(item => item.Workspace == workspace)
            .Where(item => item.Type == MessagingChannelType.Direct)
            .Where(
                item => item.CreatedBy == sender && item.User == receiver
                    || item.CreatedBy == receiver && item.User == sender
            )
            .FirstOrDefaultAsync();
        if (channel is not null)
        {
            return (channel, false);
        }

        channel = new MessagingChannelEntity
        {
            Type = MessagingChannelType.Direct,
            Workspace = workspace,
            Slug = $"{sender}-{receiver}",
            User = receiver,
            CreatedBy = sender,
            CreatedAt = DateTime.UtcNow
        };
        channel.Members = new  HashSet<MessagingChannelMemberEntity>()
        {
            new()
            {
                Member = sender,
                Channel = channel
            }
        };
        if (sender != receiver)
        {
            channel.Members.Add(new MessagingChannelMemberEntity()
            {
                Member = receiver,
                Channel = channel
            });
        }

        await Session.SaveAsync(channel);
        return (channel, true);
    }
    
    public async Task<MessagingChannelEntity> CreateChannel(
        WorkspaceEntity workspace,
        UserEntity user,
        string slug
    )
    {
        slug = slug.ToLower().Trim();
        
        var channel = await Session.Query<MessagingChannelEntity>()
            .Where(item => item.Workspace == workspace)
            .Where(item => item.Type == MessagingChannelType.Common)
            .Where(item => item.Slug == slug)
            .FirstOrDefaultAsync();
        if (channel is not null)
        {
            throw new DataValidationException($"Channel with slug {slug} already exists");
        }

        channel = new MessagingChannelEntity
        {
            Type = MessagingChannelType.Common,
            Workspace = workspace,
            Slug = slug,
            CreatedBy = user,
            CreatedAt = DateTime.UtcNow
        };
        channel.Members = new  HashSet<MessagingChannelMemberEntity>()
        {
            new()
            {
                Member = user,
                Channel = channel
            }
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
    
    public async Task<List<MessagingChannelEntity>> GetChannelsList(WorkspaceEntity workspace, UserEntity user)
    {
        return await Session.Query<MessagingChannelEntity>()
            .FetchMany(item => item.Members)
            .ThenFetch(item => item.Member)
            .Where(item => item.Workspace == workspace)
            .Where(item => item.Members.Any(m => m.Member == user && m.DeactivatedAt == null))
            .Take(200)
            .ToListAsync();
    }
}
