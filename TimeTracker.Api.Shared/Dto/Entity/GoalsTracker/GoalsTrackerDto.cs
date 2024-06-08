using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;

public class GoalsTrackerDto : IResponse
{
    public long Id { get; set; }
    
    public int Year { get; set; }
    
    public int Month { get; set; }

    public ICollection<GoalsTrackerItemDto> Items { get; set; } = new List<GoalsTrackerItemDto>();
    
    public IOrderedEnumerable<GoalsTrackerItemDto> SortedItems => Items.OrderBy(x => x.Position);
    
    public ICollection<GoalsTrackerNoteDto> Notes { get; set; } = new List<GoalsTrackerNoteDto>();

#if IS_WEB_APP
    #region Select list methods
    
    // Note: this is important so the MudSelect can compare pizzas
    public override bool Equals(object o)
    {
        var other = o as ProjectDto;
        return other?.Id == Id;
    }

    // Note: this is important too!
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    // Implement this for the Pizza to display correctly in MudSelect
    public override string ToString() => Id.ToString();
    
    #endregion
#endif
}
