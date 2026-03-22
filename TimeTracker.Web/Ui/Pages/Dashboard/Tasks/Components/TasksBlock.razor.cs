using System.Reactive.Subjects;
using System.Reactive.Linq;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksBlock: IDisposable
{
    [CascadingParameter(Name = "TaskListId")]
    public Guid? TaskListId
    {
        get => _taskListId;
        set
        {
            _taskListId = value;
            OnTasksListSelected(_taskListId);
        }
    }
    
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private Guid? _taskListId = null;
    private TaskListDto? _taskList = null;
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private ICollection<TaskDto> _tasks = new List<TaskDto>();
    private ICollection<TaskDto> _selectedTasks = new List<TaskDto>();
    private bool _isLoading = true;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        TasksState.StateChanged += OnTaskStateChanged;

        _tasksSubject
            .Select(items =>
            {
                return items
                    // .Where(item => Statuses.Contains(item.Status))
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
    
    private void OnTasksListSelected(Guid? taskListId)
    {
        if (taskListId == null)
            _taskList = null;
        else
        {
            _taskList = _tasksListState.Value.List.FirstOrDefault(item => item.Id == taskListId);
        }
    }
    
    private async Task OnAddTask()
    {
        
    }
    
    private async Task OnEditTask(TaskDto? task)
    {
    }
    
    private async Task ArchiveTasks()
    {
        // var isConfirm = await ModalDialogProviderService.ShowConfirmationDialog(
        //     "Are you sure you want to archive selected items?"
        // );
        // if (isConfirm.HasValue && isConfirm.Value)
        // {
        //     foreach (var selectedTask in _selectedTasks)
        //     {
        //         selectedTask.IsArchived = true;
        //         var updateRequest = new UpdateRequest();
        //         updateRequest.Fill(selectedTask);
        //         Dispatcher.Dispatch(new UpdateTaskAction(updateRequest, IsUpdateState: true));
        //     }
        //     _selectedTasks.Clear();    
        // }
    }

    private Task OnStatusChanged(TaskDto? task, TaskStatus? status)
    {
        return Task.CompletedTask;
    }

    private Task OnStartNewTimeEntryForTask(TaskDto? task)
    {
        return Task.CompletedTask;
    }
}
