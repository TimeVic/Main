using System.Collections;
using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;

namespace TimeTracker.Web.Store.GoalsTracker;

[FeatureState]
public record GoalsTrackerState
{
    public GoalsTrackerDto? CurrentTracker { get; set; } = null;
    
    public bool IsLoading { get; set; } = false;
}
