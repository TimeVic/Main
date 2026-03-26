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
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private TaskListDto? _taskList => _tasksListState.Value.SelectedTaskList;
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private bool _isShowAddTaskModal = false;
    private bool _isShowDeleteTaskListConfirmation = false;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();        
    }

    public void Dispose()
    {
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
    }
    
    private async Task OnAddTask()
    {
        await Task.CompletedTask;
    }
    
    private async Task OnEditTask(TaskDto? task)
    {
        await Task.CompletedTask;
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

    private Task OnEditTaskList()
    {
        return Task.CompletedTask;
    }
    
    private Task OnDeleteTaskList()
    {
        Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.ArchiveTaskListAction(_taskList!));
        return Task.CompletedTask;
    }
}
