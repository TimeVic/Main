using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using TimeTracker.Web.Store.TimeEntry;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksTableBlock: IDisposable
{
    [Parameter]
    public ICollection<TaskStatus> Statuses { get; set; }
    
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    [Inject]
    public IState<TimeEntryState> TimeEntryState { get; set; }
    
    private Guid? _taskListId = null;
    private TaskListDto? _taskList = null;
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private ICollection<TaskDto> _tasks = new List<TaskDto>();
    private ICollection<TaskDto> _selectedTasks = new List<TaskDto>();
    private bool _isLoading = true;
    private bool _isShowAddTaskModal = false;
    private bool _isDisabledButtons => TimeEntryState.Value.IsTimeEntryProcessing;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        TasksState.StateChanged += OnTaskStateChanged;
        
        _tasksSubject
            .Select(items =>
            {
                return items
                    .Where(item => Statuses.Contains(item.Status))
                    .OrderByDescending(item => item.UpdatedAt)
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
    
    private async Task OnEditTask(TaskDto? task)
    {
    }
    
    private Task OnStartNewTimeEntryForTask(TaskDto? task)
    {
        return Task.CompletedTask;
    }
    
    private Task OnStatusChanged(TaskDto? task, TaskStatus? status)
    {
        return Task.CompletedTask;
    }
    
    private void StopTimeEntry()
    {
        if (TimeEntryState.Value.HasActiveEntry)
        {
            Dispatcher.Dispatch(new StopActiveTimeEntryAction());       
        }
    }
    
    private void StartTimeEntry(TaskDto task)
    {
        Dispatcher.Dispatch(new StartTimeEntryAction(InternalTask: task));
    }
}
