using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;

namespace TimeTracker.Web.Store.Messaging.Channels;

[FeatureState]
public record ChannelsState
{
    public Guid? SelectedId { get; set; }
    
    public bool IsListLoading { get; set; } = false;
    
    public ICollection<MessagingChannelDto> List { get; set; } = new List<MessagingChannelDto>();

    #region Calculated

    public MessagingChannelDto? SelectedChannel => List.FirstOrDefault(channel => channel.Id == SelectedId);

    #endregion
}
