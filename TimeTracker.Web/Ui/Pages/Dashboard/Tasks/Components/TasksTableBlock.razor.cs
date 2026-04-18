using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Task;
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

    [Parameter]
    public bool IsLoading { get; set; }

    [Inject]
    public IState<TimeEntryState> TimeEntryState { get; set; }

    private readonly HashSet<Guid> _selectedTaskIds = [];
    private bool _isDisabledButtons => TimeEntryState.Value.IsTimeEntryProcessing;
    private TaskDto? _taskToUpdate = null;

    private static readonly IReadOnlyList<TaskStatus> taskStatusOptions = Enum.GetValues<TaskStatus>();

    // Drag-and-drop state
    private List<TaskDto> _localTasks = [];
    private TaskDto? _draggingTask = null;
    private Guid _dragOverTaskId = Guid.Empty;
    private bool _isDragging = false;
    private bool _dragRenderPending = false;

    // ondragover fires hundreds of times per second — block re-renders unless something actually changed
    protected override bool ShouldRender()
    {
        if (_isDragging && !_dragRenderPending)
            return false;
        _dragRenderPending = false;
        return true;
    }

    protected override void OnParametersSet()
    {
        if (!_isDragging)
            _localTasks = Tasks.OrderBy(t => t.PositionIndex).ToList();
    }

    private void OnDragStart(TaskDto task)
    {
        _isDragging = true;
        _draggingTask = task;
        _dragRenderPending = true;
    }

    private void OnDragOver(TaskDto task)
    {
        if (_draggingTask == null || _draggingTask.Id == task.Id || _dragOverTaskId == task.Id)
            return;
        _dragOverTaskId = task.Id;
        _dragRenderPending = true;
    }

    private void OnDrop(TaskDto targetTask)
    {
        _isDragging = false;
        _dragRenderPending = true;

        if (_draggingTask == null || _draggingTask.Id == targetTask.Id)
        {
            ResetDragState();
            return;
        }

        var fromIndex = _localTasks.IndexOf(_draggingTask);
        var toIndex = _localTasks.IndexOf(targetTask);

        if (fromIndex < 0 || toIndex < 0)
        {
            ResetDragState();
            return;
        }

        _localTasks.RemoveAt(fromIndex);
        _localTasks.Insert(toIndex, _draggingTask);

        for (var i = 0; i < _localTasks.Count; i++)
            _localTasks[i].PositionIndex = i;

        ResetDragState();
        Dispatcher.Dispatch(new UpdatePositionsAction(_localTasks));
    }

    private void OnDragEnd()
    {
        _isDragging = false;
        _dragRenderPending = true;
        ResetDragState();
    }

    private void ResetDragState()
    {
        _draggingTask = null;
        _dragOverTaskId = Guid.Empty;
    }

    private string GetDragRowClass(TaskDto task)
    {
        if (_draggingTask?.Id == task.Id)
            return "opacity-60 outline outline-2 outline-blue-400 outline-offset-[-2px] bg-blue-50/40";
        return "";
    }

    private bool IsDropTarget(TaskDto task) =>
        _isDragging && _dragOverTaskId == task.Id && _draggingTask?.Id != task.Id;

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
        if (_selectedTaskIds.Any() && _selectedTaskIds.Count == _localTasks.Count)
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

    private Task OnSetArchivedSelectedTasks(bool isArchived)
    {
        foreach (var id in _selectedTaskIds.ToList())
        {
            var task = _localTasks.FirstOrDefault(t => t.Id == id);
            if (task == null) continue;
            var updateModel = new UpdateRequest();
            updateModel.Fill(task);
            updateModel.IsArchived = isArchived;
            Dispatcher.Dispatch(new UpdateTaskAction(updateModel, true));
        }
        _selectedTaskIds.Clear();
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
