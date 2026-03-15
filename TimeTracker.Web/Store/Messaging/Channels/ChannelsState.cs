using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Channels;

[FeatureState]
public record ChannelsState
{
    public bool IsListLoading { get; set; } = false;
    
    public ICollection<MessagingChannelDto> List { get; set; } = new List<MessagingChannelDto>();
}
