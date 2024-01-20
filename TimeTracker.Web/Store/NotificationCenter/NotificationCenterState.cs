using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.NotificationCenter;

[FeatureState]
public record NotificationCenterState
{
    public int UnreadCount { get; set; } = 0;
    
    public int NextPage { get; set; } = 1;
    
    public bool IsListLoading { get; set; } = false;
    
    public bool IsListHasMore { get; set; } = true;
    
    public ICollection<NotificationDto> List { get; set; } = new List<NotificationDto>();
}
