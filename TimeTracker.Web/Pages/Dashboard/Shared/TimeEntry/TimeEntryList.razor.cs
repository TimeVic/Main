using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryList
{
    [Parameter]
    public bool IsFilteredList { get; set; } = false;
    
    [Parameter]
    public bool Outlined { get; set; } = false;
    
    [Parameter]
    public string? Class { get; set; }
    
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    [Inject] 
    private ModalDialogProviderService _modalDialogProviderService { get; set; }
    
    private ICollection<TimeEntryDto> _list => IsFilteredList ? _state.Value.FilteredList : _state.Value.ListToShow;
    
    private IEnumerable<IGrouping<DateTime, TimeEntryDto>> _groupedList => _list.GroupBy(item => item.Date);
    
    private int _totalPages => IsFilteredList ? _state.Value.FilteredTotalPages : _state.Value.TotalPages;
    
    private int _selectedPage => IsFilteredList ? _state.Value.FilteredSelectedPage : _state.Value.SelectedPage;
    
    private int _totalFilteredPages => _state.Value.FilteredTotalPages;
    
    private bool _isLoading => _state.Value.IsListLoading;

    private Func<TimeEntryDto, object> _groupBy = x =>
    {
        return x.Date.ToShortDateString();
    };
    
    private void OnSelectPage(int selectedPage)
    {
        if (IsFilteredList)
        {
            Dispatcher.Dispatch(new SetFilteredSelectedPageAction(selectedPage));
            Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());    
        }
        else
        {
            Dispatcher.Dispatch(new SetSelectedPageAction(selectedPage));
            Dispatcher.Dispatch(new LoadListAction());
        }
    }

    private string OnGroupTimeEntry(GroupDefinition<TimeEntryDto> item)
    {
        return item.Grouping.Key?.ToString() == "Nonmetal" || item.Grouping.Key?.ToString() == "Other"
            ? "mud-theme-warning"
            : string.Empty;
    }
    
    private async Task ShowAddTaskModal(long timEntryId)
    {
        await _modalDialogProviderService.ShowAddTaskModal(timEntryId);
    }

    #region On Change
    private async Task OnChangeStartTime(TimeEntryDto item, TimeSpan startTime)
    {
        item.StartTime = startTime > item.EndTime ? item.EndTime.Value : startTime;
        await UpdateTimeEntry(item);
    }

    private async Task OnChangeEndTime(TimeEntryDto item, TimeSpan endTime)
    {
        item.EndTime = endTime < item.StartTime ? item.StartTime : endTime;
        await UpdateTimeEntry(item);
    }
    
    private async Task OnChangeDescription(TimeEntryDto item, string? description)
    {
        item.Description = description;
        await UpdateTimeEntry(item);
    }
    
    private async Task OnChangeIsBillable(TimeEntryDto item, bool? isBillable)
    {
        item.IsBillable = isBillable ?? false;
        await UpdateTimeEntry(item);
    }
    
    private async Task OnChangeProject(TimeEntryDto item, ProjectDto? project)
    {
        item.Project = project;
        await UpdateTimeEntry(item);
    }
    
    private async Task UpdateTimeEntry(TimeEntryDto entry)
    {
        Dispatcher.Dispatch(new UpdateTimeEntryAction(entry));
        Dispatcher.Dispatch(new SaveTimeEntryAction(entry, false));
        await Task.CompletedTask;
    }
    
    #endregion
    
    private async Task OnDeleteItemAsync(TimeEntryDto item)
    {
        var isOk = await _modalDialogProviderService.ShowDeleteConfirmationDialog();
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new DeleteTimeEntryAction(item.Id));
        }
    }
}
