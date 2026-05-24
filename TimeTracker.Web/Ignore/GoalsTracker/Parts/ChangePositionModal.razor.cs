using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Client.Core.Store.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class ChangePositionModal
{
    [Parameter]
    public GoalsTrackerDto Tracker { get; set; }
    
    [CascadingParameter] 
    FluentDialog MudDialog { get; set; }

    public IList<GoalsTrackerItemDto> _goals { get; set; } = new List<GoalsTrackerItemDto>();
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _goals = Tracker.SortedItems.ToList();
    }

    private void Submit()
    {
        Dispatcher.Dispatch(new ChangePositionsAction(_goals));
        OnCloseModal();
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
