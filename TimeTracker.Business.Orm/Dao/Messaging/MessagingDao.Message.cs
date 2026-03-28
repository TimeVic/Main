using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
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
        var query = Session.Query<MessagingMessageEntity>()
            .Fetch(item => item.Channel)
            .Fetch(item => item.CreatedBy)
            .Where(item => item.Channel == channel);
        
        var offset = PaginationUtils.CalculateOffset(page, pageSize);
        var messages = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync();
        var count = await query.CountAsync();
        return new ListDto<MessagingMessageEntity>(messages, count);
    }
}
