using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using TimeTracker.Api.Shared.Constants.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.WebSocket.Services.Mappers;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.WebSocket.Services;

public class HubMessagingService: IHubMessagingService
{
    private readonly IMessagingDao _messagingDao;
    private readonly IHubContext<MessagingHub> _context;
    private readonly IHubMapperService _hubMapperService;
    private readonly IMapper _mapper;
    private readonly IHubMessageCountService _hubMessageCountService;

    public HubMessagingService(
        IMessagingDao messagingDao,
        IHubContext<MessagingHub> context,
        IHubMessageCountService hubMessageCountService,
        IHubMapperService hubMapperService,
        IMapper mapper
    )
    {
        _messagingDao = messagingDao;
        _context = context;
        _hubMessageCountService = hubMessageCountService;
        _hubMapperService = hubMapperService;
        _mapper = mapper;
    }

    public async Task<MessagingMessageEntity> SendMessage(
        WorkspaceEntity workspace,
        UserEntity sender,
        string messageText,
        UserEntity? receiver = null,
        MessagingChannelEntity? channel = null
    )
    {
        var recipients = new List<UserEntity>()
        {
            sender
        };
        MessagingChannelEntity messageChannel;
        if (receiver != null)
        {
            (messageChannel, _) = await _messagingDao.GetOrCreateDirectChannel(
                workspace,
                sender,
                receiver
            );
            recipients.Add(receiver);
        }
        else if (channel != null)
        {
            messageChannel = channel;
            recipients = recipients.Concat(channel.ActiveMembers.Select(item => item.Member)).ToList();
        }
        else
        {
            throw new DataValidationException("Channel can not be recognized");
        }
        var message = await _messagingDao.CreateMessage(messageChannel, sender, messageText);
        var hubRecipientConnections = await _messagingDao.GetConnectionsByUsers(recipients);
        foreach (var hubConnections in hubRecipientConnections)
        {
            var messageDto = _hubMapperService.MapToMessageDto(message, sender);
            await _context.Clients.Client(hubConnections.ConnectionId).SendAsync(HubMethodName.MessageCreated, messageDto);
        }
        
        var recipientsToIncreaseCounters = recipients.Where(item => item.Id != sender.Id).ToList();
        await _hubMessageCountService.IncreaseForUsers(messageChannel, recipientsToIncreaseCounters);
        return message;
    }
    
    public async Task<MessagingChannelEntity> CreateChannel(
        WorkspaceEntity workspace,
        UserEntity user,
        string slug,
        List<UserEntity> members
    )
    {
        var channel = await _messagingDao.CreateChannel(workspace, user, slug);
        
        var recipients = new List<UserEntity>()
        {
            user
        };
        recipients = recipients.Concat(members).ToList();
        var hubRecipientConnections = await _messagingDao.GetConnectionsByUsers(recipients);
        foreach (var hubConnections in hubRecipientConnections)
        {
            var channelDto = _mapper.Map<MessagingChannelDto>(channel);
            await _context.Clients.Client(hubConnections.ConnectionId).SendAsync(HubMethodName.ChannelCreated, channelDto);
        }

        return channel;
    }
    
    public async Task InitChannels(WorkspaceEntity workspace, UserEntity user)
    {
        var workspaceMembers = workspace.Memberships.Select(item => item.User).ToList();
        foreach (var workspaceMember in workspaceMembers)
        {
            var (channel, isCreated) = await _messagingDao.GetOrCreateDirectChannel(workspace, user, workspaceMember);
            if (!isCreated)
            {
                continue;
            }
            var hubRecipientConnections = await _messagingDao.GetConnectionsByUsers([user]);
            foreach (var hubConnections in hubRecipientConnections)
            {
                var channelDto = _mapper.Map<MessagingChannelDto>(channel);
                await _context.Clients.Client(hubConnections.ConnectionId).SendAsync(HubMethodName.ChannelCreated, channelDto);
            }
        }
    }
}
