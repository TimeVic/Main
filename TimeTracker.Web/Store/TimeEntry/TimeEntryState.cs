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

    public ICollection<TimeEntryDto> List { get; set; } = new List<TimeEntryDto>();
    
    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
}
