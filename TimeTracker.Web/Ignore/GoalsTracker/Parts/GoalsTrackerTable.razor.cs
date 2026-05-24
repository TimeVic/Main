using System.Globalization;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Client.Core.Store.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class GoalsTrackerTable
{
    [Parameter]
    public GoalsTrackerDto Tracker { get; set; }
    
    private ICollection<DateTime> _daysInCurrentMonth = new List<DateTime>();
    private DateTime _today = DateTime.Now;
    private IEnumerable<GoalsTrackerItemDto> _goals = new List<GoalsTrackerItemDto>();
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _today = DateTime.Now;
        CalculateListOdDays();
    }

    private void CalculateListOdDays()
    {
        _daysInCurrentMonth.Clear();
        for (var dayNumber = 1; dayNumber <= DateTime.DaysInMonth(Tracker.Year, Tracker.Month); dayNumber++)
        {
            _daysInCurrentMonth.Add(new DateTime(Tracker.Year, Tracker.Month, dayNumber));
        }
    }

    private void OnClickRow(GoalsTrackerItemDto goal, DateTime day)
    {
        Dispatcher.Dispatch(new SetItemCompletionAction(
            goal,
            day.Day,
            !IsSelectedRow(goal, day)
        ));
    }
    
    private bool IsSelectedRow(GoalsTrackerItemDto goal, DateTime day)
    {
        var existMarker = goal.CompletionMarkers.FirstOrDefault(item => item.DayOfMonth == day.Day);
        if (existMarker != null)
            return existMarker.IsChecked;
        return false;
    }
    
    private bool IsToday(DateTime day)
    {
        return day.Day == _today.Day && day.Month == _today.Month && day.Year == _today.Year;
    }
    
    private int GetMarkedCount(GoalsTrackerItemDto goal)
    {
        return goal.CompletionMarkers.Count(item => item.IsChecked);
    }

    private async Task OnEditGoal(GoalsTrackerItemDto goal)
    {
        await ModalDialogService.ShowGoalsTrackerUpdateItemModal(goal);
    }

    private async Task OnDeleteGoal(GoalsTrackerItemDto goal)
    {
        var isDeleted = await ModalDialogService.ShowDeleteConfirmationDialog();
        if (isDeleted.HasValue && isDeleted.Value)
        {
            Dispatcher.Dispatch(new DeleteTrackerItemAction(goal));
        }
    }
}
