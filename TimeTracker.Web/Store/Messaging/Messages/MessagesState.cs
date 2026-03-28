using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Messages;

public record MessagesListState
{
    public int Page { get; set; } = 1;
    
    public bool IsListFullListLoaded { get; set; } = false;
    
    public long TotalCount { get; set; } = 0;
    
    public required MessagingChannelDto Channel { get; set; }
    
    public ICollection<MessagingMessageDto> List { get; set; } = [];
}

[FeatureState]
public record MessagesState
{
    public bool IsMessageSending { get; set; } = false;
    
    public bool IsListLoading { get; set; } = false;
 
    public ICollection<MessagesListState> ListStates { get; set; } = [];

    public MessagesListState? GetListState(MessagingChannelDto channel)
    {
        return ListStates.FirstOrDefault(item => item.Channel == channel);
    }
}
