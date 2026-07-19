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

    private ICollection<TaskDto>? _tasksSource;
    private IReadOnlyList<TaskDto> _todoTasks = [];
    private IReadOnlyList<TaskDto> _backlogTasks = [];

    protected override void OnInitialized()
    {
        base.OnInitialized();

        TasksState.StateChanged += OnTaskStateChanged;
        UpdateTaskSections(TasksState.Value.List, false);

        ActionSubscriber.SubscribeToAction<SetIsListLoading>(this, _ => StateHasChanged());
    }

    public void Dispose()
    {
        TasksState.StateChanged -= OnTaskStateChanged;
        ActionSubscriber.UnsubscribeFromAllActions(this);
    }

    private void OnTaskStateChanged(object? sender, EventArgs e)
    {
        if (ReferenceEquals(_tasksSource, TasksState.Value.List))
        {
            return;
        }

        UpdateTaskSections(TasksState.Value.List, true);
    }

    private void UpdateTaskSections(ICollection<TaskDto> tasks, bool isRenderRequired)
    {
        _tasksSource = tasks;

        var orderedTasks = tasks
            .OrderBy(task => task.PositionIndex)
            .ThenBy(task => task.CreatedAt)
            .ToList();

        _todoTasks = orderedTasks
            .Where(task => task.Status != TaskStatus.Backlog)
            .ToList();
        _backlogTasks = orderedTasks
            .Where(task => task.Status == TaskStatus.Backlog)
            .ToList();

        if (isRenderRequired)
        {
            StateHasChanged();
        }
    }
}
