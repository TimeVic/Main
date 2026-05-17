using System.Reactive.Subjects;
using System.Reactive.Linq;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksBlock: IDisposable
{   
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private TaskListDto? _taskList => _tasksListState.Value.SelectedTaskList;
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private bool _isShowAddTaskModal = false;
    
    public void Dispose()
    {
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
    }
}
