using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksBoardBlock : IDisposable
{
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }

    [Inject]
    public IState<TasksState> TasksState { get; set; }

    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private IReadOnlyList<TaskDto> _tasks = [];

    private IReadOnlyList<TaskDto> TodoTasks => _tasks.Where(t => t.Status != TaskStatus.Backlog).ToList();
    private IReadOnlyList<TaskDto> BacklogTasks => _tasks.Where(t => t.Status == TaskStatus.Backlog).ToList();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        TasksState.StateChanged += OnTaskStateChanged;

        _tasksSubject
            .Select(items => items
                .OrderBy(t => t.PositionIndex)
                .ThenBy(t => t.CreatedAt)
                .ToList())
            .Subscribe(results =>
            {
                _tasks = results;
                StateHasChanged();
            });

        ActionSubscriber.SubscribeToAction<SetIsListLoading>(this, _ => StateHasChanged());
    }

    public void Dispose()
    {
        TasksState.StateChanged -= OnTaskStateChanged;
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
    }

    private void OnTaskStateChanged(object? sender, EventArgs e)
    {
        _tasksSubject.OnNext(TasksState.Value.List);
    }
}
