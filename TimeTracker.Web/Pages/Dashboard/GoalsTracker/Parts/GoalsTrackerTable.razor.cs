using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class GoalsTrackerTable
{
    [Parameter]
    public int Year { get; set; }

    [Parameter]
    public int Month { get; set; }
    
    private ICollection<DateTime> _daysInCurrentMonth = new List<DateTime>();
    
    private ICollection<string> _goals = new List<string>()
    {
        "goal1",
        "goal2",
        "goal3",
        "goal4",
        "goal5",
        "goal6",
        "goal7",
        "goal8",
        "goal9",
        "goal10",
        "goal11",
        "goal12",
    };
    
    private IDictionary<string, ICollection<DateTime>> _completedItems = new Dictionary<string, ICollection<DateTime>>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        CalculateListOdDays();
    }

    private void CalculateListOdDays()
    {
        _daysInCurrentMonth.Clear();
        for (var dayNumber = 1; dayNumber <= DateTime.DaysInMonth(Year, Month); dayNumber++)
        {
            _daysInCurrentMonth.Add(new DateTime(Year, Month, dayNumber));
        }
    }

    private async Task OnAddGoal()
    {
        await Task.CompletedTask;
    }

    private void OnClickRow(string goal, DateTime day)
    {
        if (!_completedItems.ContainsKey(goal))
        {
            _completedItems.Add(goal, new List<DateTime>());
        }

        _completedItems.TryGetValue(goal, out var selectedItems);
        var selectedItem = selectedItems!.FirstOrDefault(item => item == day);
        if (selectedItem != DateTime.MinValue)
        {
            selectedItems = selectedItems!.Where(item => item != day).ToList();
        }
        else
        {
            selectedItems!.Add(day);
        }
        _completedItems[goal] = selectedItems;
    }
    
    private bool IsSelectedRow(string goal, DateTime day)
    {
        if (!_completedItems.ContainsKey(goal))
        {
            return false;
        }
        _completedItems.TryGetValue(goal, out var selectedItems);
        return selectedItems!.Any(item => item == day);
    }
}
