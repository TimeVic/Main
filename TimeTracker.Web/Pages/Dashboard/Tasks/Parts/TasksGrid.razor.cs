using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksGrid: IDisposable
{
    [Parameter]
    public TaskStatus Status { get; set; }
    
    [Inject]
    public ModalDialogProviderService ModalDialogProviderService { get; set; }

    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }

    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private ICollection<TaskDto> _tasks = new List<TaskDto>();
    private bool _isLoading = true;
    private long? _taskListId;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        TasksState.StateChanged += OnTaskStateChanged;

        _tasksSubject
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Select(items =>
            {
                return items.Where(item => item.Status == Status)
                    .OrderByDescending(item => item.UpdateTime)
                    .ToArray();
            })
            .Subscribe(results =>
            {
                _tasks = results;
                StateHasChanged();
            });
        
        ActionSubscriber.SubscribeToAction<TimeTracker.Web.Store.Tasks.SetIsListLoading>(this, action =>
        {
            _isLoading = action.IsLoading;
            StateHasChanged();
        });
        ActionSubscriber.SubscribeToAction<TimeTracker.Web.Store.TasksList.SetSelectedAction>(this, action =>
        {
            _taskListId = action.TaskListId;
            StateHasChanged();
        });
    }

    private void OnTaskStateChanged(object? sender, EventArgs e)
    {
        _tasksSubject.OnNext(TasksState.Value.List);
    }

    private async Task OnAddTask()
    {
        await ModalDialogProviderService.ShowAddTaskModal(
            taskListId: _taskListId,
            taskStatus: Status
        );
    }
    
    public void Dispose()
    {
        TasksState.StateChanged -= OnTaskStateChanged;
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
    }
}
