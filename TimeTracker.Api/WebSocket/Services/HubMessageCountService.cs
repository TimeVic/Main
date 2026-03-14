using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.WebSocket.Constants;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.WebSocket.Services;

public class HubMessageCountService: IHubMessageCountService
{
    private readonly IMessagingDao _messagingDao;
    private readonly IDbSessionProvider _dbSessionProvider;
    private readonly IHubContext<MessagingHub> _context;
    private readonly IMapper _mapper;

    public HubMessageCountService(
        IMessagingDao messagingDao,
        IDbSessionProvider dbSessionProvider,
        IHubContext<MessagingHub> context,
        IMapper mapper
    )
    {
        _messagingDao = messagingDao;
        _dbSessionProvider = dbSessionProvider;
        _context = context;
        _mapper = mapper;
    }
    
    public async Task IncreaseForUser(MessagingChannelEntity channel, UserEntity user)
    {
        var countEntity = await _messagingDao.IncreaseForUser(channel, user);

        var userConnections =  await _messagingDao.GetConnectionsByUsers([user]);
        foreach (var connection in userConnections)
        {
            await _context.Clients.Client(connection.ConnectionId).SendAsync(
                HubMethodName.MessageCounterUpdated,
                _mapper.Map<MessagingMessageCountDto>(countEntity)
            );
        }
    }
    
    public async Task IncreaseForUsers(MessagingChannelEntity channel, IList<UserEntity> users)
    {
        foreach (var user in users)
        {
            await IncreaseForUser(channel, user);
        }
    }
    
    public async Task ResetForUser(MessagingChannelEntity channel, UserEntity user)
    {
        var countEntity = await _messagingDao.GetMessageCount(channel, user);
        if (countEntity is not null)
        {
            countEntity.Counter = 0;
            countEntity.UpdatedAt = DateTime.UtcNow;
            await _dbSessionProvider.CurrentSession.SaveOrUpdateAsync(countEntity);
            
            var userConnections =  await _messagingDao.GetConnectionsByUsers([user]);
            foreach (var connection in userConnections)
            {
                await _context.Clients.Client(connection.ConnectionId).SendAsync(
                    HubMethodName.MessageCounterUpdated,
                    _mapper.Map<MessagingMessageCountDto>(countEntity)
                );
            }
        }
    }
}
