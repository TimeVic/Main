using Microsoft.AspNetCore.SignalR.Client;
using TimeTracker.Api.Shared.Constants.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Client.Core.Services.Messaging;

public partial class MessagingWebSocketClientService
{
    public Action<MessagingChannelDto> OnChannelCreated { get; set; } = _ => { };
    public Action<MessagingMessageDto> OnMessageCreated { get; set; } = _ => { };
    public Action<MessagingMessageCountDto> OnCounterUpdated { get; set; } = _ => { };
    
    private void InitEvents()
    {
        _hubConnection?.On<MessagingChannelDto>(HubMethodName.ChannelCreated, (entity) =>
        {
            _logger.LogDebug($"WebSocket event: {HubMethodName.ChannelCreated}: {entity.Id}");
            OnChannelCreated.Invoke(entity);
        });
        
        _hubConnection?.On<MessagingMessageDto>(HubMethodName.MessageCreated, (entity) =>
        {
            _logger.LogDebug($"WebSocket event: {HubMethodName.MessageCreated}: {entity.Id}");
            OnMessageCreated.Invoke(entity);
        });
        
        _hubConnection?.On<MessagingMessageCountDto>(HubMethodName.MessageCounterUpdated, (entity) =>
        {
            _logger.LogDebug($"WebSocket event: {HubMethodName.MessageCounterUpdated}: {entity.Channel.Id}");
            OnCounterUpdated.Invoke(entity);
        });
    }
}
