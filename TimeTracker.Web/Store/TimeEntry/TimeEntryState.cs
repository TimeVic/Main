using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.TimeEntry;

[FeatureState]
public record TimeEntryState
{
    public bool HasActiveEntry
    {
        get => ActiveEntry != null;
    }
    
    public TimeEntryDto? ActiveEntry { get; set; }
}
