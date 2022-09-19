using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.List;

public partial class TimeEntryList
{
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private RadzenDataGrid<TimeEntryDto> _grid;
    private TimeEntryDto _modelToEdit = new();

    private Task OnLoadList(LoadDataArgs arg)
    {
        Dispatcher.Dispatch(new LoadTimeEntryListAction(arg.Skip ?? 0));
        return Task.CompletedTask;
    }

    private void OnChangeStartTime(TimeEntryDto item, TimeSpan startTime)
    {
        item.StartTime = startTime > item.EndTime ? item.EndTime.Value : startTime;
    }

    private async Task OnChangeEndTime(TimeEntryDto item, TimeSpan endTime)
    {
        item.EndTime = endTime < item.StartTime ? item.StartTime : endTime;
        await Task.CompletedTask;
    }

    private async Task OnChangeItemDateAsync(TimeEntryDto item, DateTime? date)
    {
        if (!date.HasValue)
            return;
        item.Date = date.Value;
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new UpdateTimeEntryAction(entry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry, false));
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
    
    private async Task EditRow(TimeEntryDto item)
    {
        _modelToEdit.UpdateFrom(item);
        await _grid.EditRow(item);
    }

    private async Task OnClickSaveRow(TimeEntryDto item)
    {
        await _grid.UpdateRow(item);
    }

    private void OnClickCancelEditMode(TimeEntryDto item)
    {
        Debug.Log("OnClickCancelEditMode");
        item.UpdateFrom(_modelToEdit);
        _grid.CancelEditRow(item);
    }
    
    private async Task OnUpdateRow(TimeEntryDto item)
    {
        Debug.Log("OnUpdateRow");
        await UpdateTimeEntry(item);
    }
}
