using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.Messaging;

public partial class MessagingDao
{
    public async Task<MessagingMessageEntity> CreateMessage(MessagingChannelEntity channel, UserEntity user, string message)
    {
        var messageEntity = new MessagingMessageEntity()
        {
            Text = message,
            Channel = channel,
            CreatedBy = user,
            CreatedAt = DateTime.UtcNow
        };
        await Session.SaveAsync(messageEntity);
        return messageEntity;
    }
    
    public async Task<ListDto<MessagingMessageEntity>> GetMessagesList(MessagingChannelEntity channel, int page, int pageSize = GlobalConstants.DefaultListPageSize)
    {
        MessagingChannelEntity? channelAlias = null;
        UserEntity? senderAlias = null, customerAlias = null;
        var query = Session.QueryOver<MessagingMessageEntity>()
            .Inner.JoinAlias(item => item.Channel, () => channelAlias)
            .Inner.JoinAlias(item => item.CreatedBy, () => senderAlias)
            .Where(item => item.Channel == channel);
        
        var messages = await query
            .Clone()
            .OrderBy(item => item.CreatedAt).Desc
            .ListAsync();
        var count = await query.Clone()
            .RowCountAsync();
        return new ListDto<MessagingMessageEntity>(messages, count);
    }
}
