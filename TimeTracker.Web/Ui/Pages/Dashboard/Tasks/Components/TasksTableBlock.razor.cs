using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TimeEntry;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksTableBlock
{
    [Parameter]
    public IReadOnlyList<TaskDto> Tasks { get; set; } = [];

    [Parameter]
    public string EmptyMessage { get; set; } = "No tasks found.";

    [Inject]
    public IState<TimeEntryState> TimeEntryState { get; set; }

    private readonly HashSet<Guid> _selectedTaskIds = new();
    private bool _isDisabledButtons => TimeEntryState.Value.IsTimeEntryProcessing;
    private TaskDto? _taskToUpdate = null;

    private static readonly IReadOnlyList<TaskStatus> taskStatusOptions = Enum.GetValues<TaskStatus>();

    private void OpenTaskEditor(TaskDto task)
    {
        _taskToUpdate = task;
    }

    private void StartTimeEntry(TaskDto task)
    {
        Dispatcher.Dispatch(new StartTimeEntryAction(InternalTask: task));
    }

    private void StopTimeEntry()
    {
        if (TimeEntryState.Value.HasActiveEntry)
            Dispatcher.Dispatch(new StopActiveTimeEntryAction());
    }

    private bool IsTaskBoardSelected(Guid id) => _selectedTaskIds.Contains(id);

    private void ToggleTaskBoardSelection(Guid id)
    {
        if (!_selectedTaskIds.Remove(id))
            _selectedTaskIds.Add(id);
    }

    private void SelectAllTasks()
    {
        if (_selectedTaskIds.Any())
        {
            _selectedTaskIds.Clear();
            return;
        }
        _selectedTaskIds.Clear();
        foreach (var task in Tasks)
        {
            _selectedTaskIds.Add(task.Id);
        }
    }

    private string GetTaskBoardRowClass(Guid id) =>
        _selectedTaskIds.Contains(id) ? "bg-blue-50/50" : "hover:bg-slate-50/50 group";

    private Task OnArchiveTask(TaskDto task)
    {
        var updateModel = new UpdateRequest();
        updateModel.Fill(task);
        updateModel.IsArchived = true;
        Dispatcher.Dispatch(new UpdateTaskAction(updateModel, true));
        return Task.CompletedTask;
    }
    
    private Task OnChangeStatusTask(TaskDto task, TaskStatus? status)
    {
        if (status != null)
        {
            var updateModel = new UpdateRequest();
            updateModel.Fill(task);
            updateModel.Status = status.Value;
            Dispatcher.Dispatch(new UpdateTaskAction(updateModel, true));
        }
        return Task.CompletedTask;
    }

    private Task OnChangePriorityTask(TaskDto task, TaskPriority? priority)
    {
        if (priority != null)
        {
            var updateModel = new UpdateRequest();
            updateModel.Fill(task);
            updateModel.Priority = priority.Value;
            Dispatcher.Dispatch(new UpdateTaskAction(updateModel, true));
        }
        return Task.CompletedTask;
    }
    
    private string GetTaskTitleClass(TaskDto context)
    {
        return context.IsArchived ? "line-through" : "";
    }
}
