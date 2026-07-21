using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksBoardBlock : IDisposable
{
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }

    [Inject]
    public IState<TasksState> TasksState { get; set; }

    // Keep these instances stable so task updates do not reset Virtualize's cached item range.
    private readonly List<TaskDto> _todoTasks = [];
    private readonly List<TaskDto> _backlogTasks = [];
    private long _todoTasksVersion;
    private long _backlogTasksVersion;
    private bool _isRenderPending = true;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UpdateTaskSections(TasksState.Value.List);

        ActionSubscriber.SubscribeToAction<SetListItemsAction>(this, action =>
        {
            UpdateTaskSections(action.Response.Items);
            RequestRender();
        });
        ActionSubscriber.SubscribeToAction<SetListItemAction>(this, action =>
        {
            UpdateTask(action.Task);
            RequestRender();
        });
        ActionSubscriber.SubscribeToAction<RemoveListItemAction>(this, action =>
        {
            if (RemoveTask(action.TaskId))
            {
                RequestRender();
            }
        });
        ActionSubscriber.SubscribeToAction<UpdateListItemsAction>(this, action =>
        {
            var isUpdated = false;
            foreach (var task in action.Tasks)
            {
                isUpdated |= UpdateTask(task);
            }

            if (isUpdated)
            {
                RequestRender();
            }
        });
        ActionSubscriber.SubscribeToAction<SetIsListLoading>(this, _ => RequestRender());
    }

    public void Dispose()
    {
        ActionSubscriber.UnsubscribeFromAllActions(this);
    }

    protected override bool ShouldRender()
    {
        if (!_isRenderPending)
        {
            return false;
        }

        _isRenderPending = false;
        return true;
    }

    private void UpdateTaskSections(ICollection<TaskDto> tasks)
    {
        var orderedTasks = tasks
            .OrderBy(task => task.PositionIndex)
            .ThenBy(task => task.CreatedAt);

        _todoTasks.Clear();
        _backlogTasks.Clear();

        foreach (var task in orderedTasks)
        {
            GetTaskSection(task).Add(task);
        }

        _todoTasksVersion++;
        _backlogTasksVersion++;
    }

    private bool UpdateTask(TaskDto task)
    {
        var currentSection = FindTaskSection(task.Id, out var taskIndex);
        var targetSection = GetTaskSection(task);

        if (currentSection == null)
        {
            InsertTask(targetSection, task);
            IncrementVersion(targetSection);
            return true;
        }

        if (ReferenceEquals(currentSection, targetSection))
        {
            var currentTask = currentSection[taskIndex];
            currentSection[taskIndex] = task;

            if (currentTask.PositionIndex != task.PositionIndex || currentTask.CreatedAt != task.CreatedAt)
            {
                currentSection.RemoveAt(taskIndex);
                InsertTask(currentSection, task);
            }

            IncrementVersion(currentSection);
            return true;
        }

        currentSection.RemoveAt(taskIndex);
        InsertTask(targetSection, task);
        IncrementVersion(currentSection);
        IncrementVersion(targetSection);
        return true;
    }

    private bool RemoveTask(Guid taskId)
    {
        var taskSection = FindTaskSection(taskId, out var taskIndex);
        if (taskSection == null)
        {
            return false;
        }

        taskSection.RemoveAt(taskIndex);
        IncrementVersion(taskSection);
        return true;
    }

    private List<TaskDto> GetTaskSection(TaskDto task) => task.Status == TaskStatus.Backlog
        ? _backlogTasks
        : _todoTasks;

    private List<TaskDto>? FindTaskSection(Guid taskId, out int taskIndex)
    {
        taskIndex = _todoTasks.FindIndex(task => task.Id == taskId);
        if (taskIndex >= 0)
        {
            return _todoTasks;
        }

        taskIndex = _backlogTasks.FindIndex(task => task.Id == taskId);
        return taskIndex >= 0 ? _backlogTasks : null;
    }

    private static void InsertTask(List<TaskDto> tasks, TaskDto task)
    {
        var taskIndex = tasks.BinarySearch(task, TaskPositionComparer.Instance);
        tasks.Insert(taskIndex < 0 ? ~taskIndex : taskIndex, task);
    }

    private void IncrementVersion(List<TaskDto> tasks)
    {
        if (ReferenceEquals(tasks, _todoTasks))
        {
            _todoTasksVersion++;
            return;
        }

        _backlogTasksVersion++;
    }

    private void RequestRender()
    {
        _isRenderPending = true;
        _ = InvokeAsync(StateHasChanged);
    }

    private sealed class TaskPositionComparer : IComparer<TaskDto>
    {
        public static readonly TaskPositionComparer Instance = new();

        public int Compare(TaskDto? left, TaskDto? right)
        {
            var positionComparison = left!.PositionIndex.CompareTo(right!.PositionIndex);
            return positionComparison != 0
                ? positionComparison
                : left.CreatedAt.CompareTo(right.CreatedAt);
        }
    }
}
