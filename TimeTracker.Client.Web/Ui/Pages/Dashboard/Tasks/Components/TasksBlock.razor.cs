using System.Reactive.Subjects;
using System.Reactive.Linq;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.Tasks;
using TimeTracker.Client.Core.Store.TasksList;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksBlock: IDisposable
{   
    [Parameter]
    public bool IsEmbedded { get; set; }

    [Parameter]
    public ClientDto? ContextClient { get; set; }

    [Parameter]
    public ProjectDto? ContextProject { get; set; }

    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private TaskListDto? _taskList => _tasksListState.Value.SelectedTaskList;
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private bool _isShowAddTaskModal = false;
    private bool _isShowAddTaskListModal = false;

    private string ContainerClass => IsEmbedded
        ? "flex min-h-[720px] w-full flex-col bg-white"
        : "space-y-4";

    private string HeaderClass => IsEmbedded
        ? "border-b border-slate-200 p-4"
        : "px-1";

    private string BoardClass => IsEmbedded
        ? "min-h-0 flex-1 p-4"
        : string.Empty;

    private string EmptyStateClass => IsEmbedded
        ? "m-4 border border-dashed border-slate-200 bg-slate-50 px-5 py-6"
        : "border border-dashed border-slate-200 bg-slate-50 px-5 py-6";
    
    public void Dispose()
    {
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
    }

    private string GetTaskListContext() =>
        $"{_taskList?.Project.Client?.Name ?? DashboardLocalizer["NoClient"].Value} · {_taskList?.Project.Name}";

    private Task OnTaskListAdded(TaskListDto? taskList)
    {
        if (taskList != null)
        {
            NavigationManager.NavigateTo(UrlService.GetDashboardUrl($"tasks/{taskList.Id}"));
        }

        return Task.CompletedTask;
    }
}
