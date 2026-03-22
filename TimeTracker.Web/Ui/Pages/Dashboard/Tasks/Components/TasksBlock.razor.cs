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
    private bool _isShowAddTaskModal = false;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _tasksListState.StateChanged += OnTasksListStateChanged;        
    }

    private void OnTasksListStateChanged(object? sender, EventArgs e)
    {
        OnTasksListSelected(_tasksListState.Value.SelectedTaskList?.Id);
    }

    public void Dispose()
    {
        _tasksListState.StateChanged -= OnTasksListStateChanged;
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
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
}
