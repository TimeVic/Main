using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Messages;

[FeatureState]
public record MessagesState
{
    public int Page { get; set; } = 1;
    
    public bool IsMessageSending { get; set; } = false;
    
    public bool IsListLoading { get; set; } = false;
    
    public long TotalCount { get; set; } = 0;
    
    public ICollection<MessagingMessageDto> List { get; set; } = new List<MessagingMessageDto>();
}
