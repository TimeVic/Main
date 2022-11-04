using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;

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
    
    public ICollection<TimeEntryDto> ListToShow => List.Where(item => !item.IsActive).ToList();

    public int TotalCount { get; set; }
    
    public int TotalPages { get; set; }
    
    public bool HasMoreItems { get; set; }
    
    public bool IsListLoading { get; set; }

    #region Filtered
    
    public TimeEntryFilterState Filter { get; set; } = new();
    
    public ICollection<TimeEntryDto> FilteredList { get; set; } = new List<TimeEntryDto>();
    
    public int FilteredTotalCount { get; set; }
    
    public int FilteredTotalPages { get; set; }
    
    public bool FilteredHasMoreItems { get; set; }
    
    #endregion
}

public record TimeEntryFilterState
{
    public long? ClientId { get; set; }
    
    public long? ProjectId { get; set; }
    
    public long? UserId { get; set; }

    public string? Search { get; set; } = "";
    
    public DateTime? DateFrom { get; set; }
    
    public DateTime? DateTo { get; set; } = DateTime.Now.EndOfDay();
    
    public bool? IsBillable { get; set; }
}
