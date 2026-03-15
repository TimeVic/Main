using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Messages;

[FeatureState]
public record MessagesState
{
    public bool IsListLoading { get; set; } = false;
    
    public ICollection<MessagingMessageDto> List { get; set; } = new List<MessagingMessageDto>();
}
