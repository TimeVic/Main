using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.TimeEntry;
using TimeTracker.Client.Core.Ui.Shared.Components.Enums;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

public partial class TaskTimeTrackerButton
{
    [Parameter]
    public required TaskDto Task { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool IconOnly { get; set; } = true;

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Small;

    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Outlined;

    [Inject]
    public IState<TimeEntryState> TimeEntryState { get; set; } = null!;

    private bool IsCurrentTaskTimerActive => TimeEntryState.Value.ActiveEntry?.Task?.Id == Task.Id;
    private bool IsDisabled => TimeEntryState.Value.IsTimeEntryProcessing;
    private string ButtonTitle => IsCurrentTaskTimerActive
        ? DashboardLocalizer["StopTimer"].Value
        : DashboardLocalizer["StartTimer"].Value;

    private void StartTimeEntry()
    {
        Dispatcher.Dispatch(new StartTimeEntryAction(InternalTask: Task));
    }

    private void StopTimeEntry()
    {
        if (TimeEntryState.Value.HasActiveEntry)
        {
            Dispatcher.Dispatch(new StopActiveTimeEntryAction());
        }
    }
}
