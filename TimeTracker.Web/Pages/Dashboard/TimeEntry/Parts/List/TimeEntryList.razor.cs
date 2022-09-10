using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.TimeEntry;
using TimeTracker.Business.Extensions;

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

    private async Task OnChangeStartTime(TimeEntryDto item, TimeSpan time)
    {
        item.StartTime = item.StartTime.Add(time);
        await UpdateTimeEntry(item);
        await Task.CompletedTask;
    }

    private async Task OnChangeEndTime(TimeEntryDto item, TimeSpan time)
    {
        item.EndTime = item.EndTime.Value.Add(time);
        await UpdateTimeEntry(item);
        await Task.CompletedTask;
    }

    private async Task OnChangeDescription(TimeEntryDto item, string value)
    {
        item.Description = value;
        await UpdateTimeEntry(item);
        await Task.CompletedTask;
    }
    
    private async Task OnChangeProject(TimeEntryDto item, ProjectDto project)
    {
        item.Project = project;
        await UpdateTimeEntry(item);
        await Task.CompletedTask;
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new UpdateTimeEntryAction(entry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry));
        await Task.CompletedTask;
    }
}
