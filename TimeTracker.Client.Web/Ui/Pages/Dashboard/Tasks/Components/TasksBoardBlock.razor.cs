using Fluxor;
using LumexUI;
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
    private readonly List<TaskDto> _inProgressTasks = [];
    private readonly List<TaskDto> _todoTasks = [];
    private readonly List<TaskDto> _backlogTasks = [];
    private readonly List<TaskDto> _doneTasks = [];
    private long _inProgressTasksVersion;
    private long _todoTasksVersion;
    private long _backlogTasksVersion;
    private long _doneTasksVersion;
    private bool _isInProgressExpanded = true;
    private bool _isTodoExpanded = true;
    private bool _isBacklogExpanded = true;
    private bool _isDoneExpanded = true;
    private bool _isRenderPending = true;

    private static readonly AccordionItemSlots TaskStatusBlockClasses = new()
    {
        Base = "rounded-lg border border-slate-200 bg-white",
        Trigger = "border-b border-slate-200 px-3 py-2.5",
        Title = "text-sm font-semibold text-slate-900",
        Subtitle = "mt-0.5 text-sm text-slate-500",
        Content = "p-0",
        Indicator = "text-slate-400 transition-transform duration-200"
    };

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

        foreach (var taskSection in GetTaskSections())
        {
            taskSection.Clear();
        }

        foreach (var task in orderedTasks)
        {
            GetTaskSection(task).Add(task);
        }

        _inProgressTasksVersion++;
        _todoTasksVersion++;
        _backlogTasksVersion++;
        _doneTasksVersion++;
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

    private List<TaskDto> GetTaskSection(TaskDto task) => task.Status switch
    {
        TaskStatus.InProgress => _inProgressTasks,
        TaskStatus.Backlog => _backlogTasks,
        TaskStatus.Done => _doneTasks,
        _ => _todoTasks
    };

    private List<TaskDto>? FindTaskSection(Guid taskId, out int taskIndex)
    {
        foreach (var taskSection in GetTaskSections())
        {
            taskIndex = taskSection.FindIndex(task => task.Id == taskId);
            if (taskIndex >= 0)
            {
                return taskSection;
            }
        }

        taskIndex = -1;
        return null;
    }

    private static void InsertTask(List<TaskDto> tasks, TaskDto task)
    {
        var taskIndex = tasks.BinarySearch(task, TaskPositionComparer.Instance);
        tasks.Insert(taskIndex < 0 ? ~taskIndex : taskIndex, task);
    }

    private void IncrementVersion(List<TaskDto> tasks)
    {
        if (ReferenceEquals(tasks, _inProgressTasks))
        {
            _inProgressTasksVersion++;
            return;
        }

        if (ReferenceEquals(tasks, _todoTasks))
        {
            _todoTasksVersion++;
            return;
        }

        if (ReferenceEquals(tasks, _backlogTasks))
        {
            _backlogTasksVersion++;
            return;
        }

        _doneTasksVersion++;
    }

    private IEnumerable<List<TaskDto>> GetTaskSections()
    {
        yield return _inProgressTasks;
        yield return _todoTasks;
        yield return _backlogTasks;
        yield return _doneTasks;
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
