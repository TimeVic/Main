using Autofac;
using NHibernate.Linq;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.Messaging;

public partial class MessagingDao: BaseDao, IMessagingDao
{
    public MessagingDao(ILifetimeScope scope) : base(scope)
    {
    }
    
    public async Task<MessagingConnectionEntity> SetConnection(UserEntity user, string connectionId)
    {
        await DeleteConnection(user, connectionId);
        var connection = new MessagingConnectionEntity()
        {
            ConnectionId = connectionId,
            User = user,
            CreatedAt = DateTime.UtcNow
        };
        await Session.SaveAsync(connection);
        return connection;
    }
    
    public async Task DeleteConnection(UserEntity user, string connectionId)
    {
        var existConnection = await GetConnection(user, connectionId);
        if (existConnection is not null)
        {
            await Session.DeleteAsync(existConnection);
        }
    }
    
    public async Task<MessagingConnectionEntity?> GetConnection(UserEntity user, string connectionId)
    {
        return await Session.Query<MessagingConnectionEntity>()
            .Where(c => c.ConnectionId == connectionId && c.User == user)
            .FirstOrDefaultAsync();
    }
    
    public async Task<IList<MessagingConnectionEntity>> GetConnectionsByUsers(IList<UserEntity> users)
    {
        var userIds = users.Select(u => u.Id).ToList();
        if (!userIds.Any())
        {
            return [];
        }
        return await Session.Query<MessagingConnectionEntity>()
            .Fetch(item => item.User)
            .Where(item => userIds.Contains(item.User.Id))
            .ToListAsync();
    }
}
