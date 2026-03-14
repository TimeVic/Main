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
    public async Task<MessagingCounterEntity?> GetMessageCount(MessagingChannelEntity channel, UserEntity user)
    {
        return await Session.Query<MessagingCounterEntity>()
            .Fetch(item => item.User)
            .Fetch(item => item.Channel)
            .Where(item => item.User == user && item.Channel == channel)
            .Take(1)
            .SingleOrDefaultAsync();
    }
    
    public async Task<IList<MessagingCounterEntity>> GetMessageCounters(UserEntity user)
    {
        return await Session.Query<MessagingCounterEntity>()
            .Fetch(item => item.Channel)
            .Fetch(item => item.User)
            .Where(item => item.User == user)
            .ToListAsync();
    }
    
    public async Task<MessagingCounterEntity> IncreaseForUser(MessagingChannelEntity channel, UserEntity user)
    {
        var countEntity = user.MessageCounters.FirstOrDefault(item => item.Channel == channel);
        if (countEntity is null)
        {
            countEntity = new MessagingCounterEntity
            {
                Channel = channel,
                User = user,
                Counter = 0,
                CreatedAt = DateTime.UtcNow
            };
            user.MessageCounters.Add(countEntity);
        }

        countEntity.Counter++;
        countEntity.UpdatedAt = DateTime.UtcNow;
        await Session.SaveOrUpdateAsync(user);
        return countEntity;
    }
}
