using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryList
{
    [Parameter]
    public bool IsFilteredList { get; set; } = false;
    
    [Parameter]
    public string? Class { get; set; }
    
    [CascadingParameter] 
    public required FluentDialog MudDialog { get; set; }
    
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    [Inject] 
    private ModalDialogProviderService _modalDialogProviderService { get; set; }
    
    private ICollection<TimeEntryDto> _list => IsFilteredList ? _state.Value.FilteredList : _state.Value.ListToShow;

    private IEnumerable<IGrouping<DateOnly, TimeEntryDto>> _groupedList => _list.GroupBy(item => item.Date);
    
    private int _totalCount => IsFilteredList ? _state.Value.FilteredTotalCount : _state.Value.TotalCount;
    
    private int _selectedPage => IsFilteredList ? _state.Value.FilteredSelectedPage : _state.Value.SelectedPage;
    
    private bool _isLoading => _state.Value.IsListLoading;

    private readonly PaginationState _paginationState;

    private Func<TimeEntryDto, object> _groupBy = x =>
    {
        return x.Date.ToShortDateString();
    };

    public TimeEntryList()
    {
        _paginationState = new()
        {
            ItemsPerPage = GlobalConstants.ListPageSize
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _state.StateChanged += (sender, args) =>
        {
            _paginationState.SetTotalItemCountAsync(_totalCount);
        };
        _paginationState.SetTotalItemCountAsync(_totalCount);
    }

    private void OnPaginated(int selectedPageIndex)
    {
        var selectedPage = selectedPageIndex + 1;
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
    
    private async Task ShowAddTaskModal(Guid timEntryId)
    {
        await _modalDialogProviderService.ShowAddTaskModal(timEntryId);
    }

    private async Task OnDeleteItemAsync(TimeEntryDto item)
    {
        var isOk = await _modalDialogProviderService.ShowDeleteConfirmationDialog();
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new DeleteTimeEntryAction(item.Id));
        }
    }
    
    private string? GetDescriptionLabel(TimeEntryDto timeEntry)
    {
        if (string.IsNullOrEmpty(timeEntry.Description) && timeEntry.Task != null)
        {
            return timeEntry.Task.Title.TruncateAndAddDots(20);
        }
        return timeEntry.Description?.TruncateAndAddDots(20);
    }
    
    private void OnStartCloned(TimeEntryDto timeEntry)
    {
        Dispatcher.Dispatch(
            new StartTimeEntryAction(
                IsBillable: timeEntry.IsBillable,
                Project: timeEntry.Project,
                Description: timeEntry.Description,
                HourlyRate: timeEntry.HourlyRate,
                InternalTask: timeEntry.Task
            )
        );
        MudDialog?.CloseAsync();
    }

    private async Task OnEditClick(TimeEntryDto timeEntry)
    {
        await _modalDialogProviderService.ShowTimeEntryEditModal(timeEntry);
    }
}
