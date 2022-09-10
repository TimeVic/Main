using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.List;

public partial class TimeEntryList
{
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    private RadzenDataGrid<TimeEntryDto> _grid;
    
    private Task OnLoadList(LoadDataArgs arg)
    {
        Dispatcher.Dispatch(new LoadTimeEntryListAction(arg.Skip ?? 0));
        return Task.CompletedTask;
    }

    private async Task OnChangeStartTime(TimeSpan time)
    {
        await Task.CompletedTask;
    }

    private async Task OnChangeEndTime(TimeSpan time)
    {
        await Task.CompletedTask;
    }
}
