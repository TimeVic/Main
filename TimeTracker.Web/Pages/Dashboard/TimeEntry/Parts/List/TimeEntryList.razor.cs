using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;

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

    private async Task OnChangeStartTime(TimeEntryDto item, TimeSpan startTime)
    {
        item.StartTime = startTime > item.EndTime ? item.EndTime.Value : startTime;
        await UpdateTimeEntry(item);
        await Task.CompletedTask;
    }

    private async Task OnChangeEndTime(TimeEntryDto item, TimeSpan endTime)
    {
        item.EndTime = endTime < item.StartTime ? item.StartTime : endTime;
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
    
    private async Task OnChangeItemDateAsync(TimeEntryDto item, DateTime? date)
    {
        if (!date.HasValue)
            return;
        item.Date = date.Value;
        await UpdateTimeEntry(item);
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new UpdateTimeEntryAction(entry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry));
        await Task.CompletedTask;
    }

    private async Task OnDeleteItemAsync(TimeEntryDto item)
    {
        var isOk = await DialogService.Confirm(
            "Are you sure you want to remove this item?",
            "Delete confirmation",
            new ConfirmOptions()
            {
                OkButtonText = "Delete",
                CancelButtonText = "Cancel"
            }
        );
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new DeleteTimeEntryAction(item.Id));
        }
    }
}
