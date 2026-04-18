using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;

public class GoalsTrackerNoteDto : BaseDto
{   
    public string Text { get; set; } = string.Empty;
    
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
    public override string ToString() => Text;
    
    #endregion
#endif
}
