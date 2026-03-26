using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks;

public partial class OverdueTasksPage
{
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private ICollection<TaskDto> _tasks = new List<TaskDto>();
    private bool _isLoading = false;
    
    private DateTime _listStartDate = DateTime.Now.Date;
    private DateTime _listEndDate = DateTime.Now.Date.AddMonths(6);

    private ICollection<TaskDto> _overdueTasks => _tasks.Where(item => item.DueTime < _listStartDate).ToList();
    
    private ICollection<TaskDto> _featureTasks => _tasks.Where(item => item.DueTime >= _listStartDate).ToList();

    private IEnumerable<IGrouping<DateTime?, TaskDto>> _groupedTasks
    {
        get
        {
            return _featureTasks.GroupBy(item => item.DueTime?.Date);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        TasksState.StateChanged += (sender, args) =>
        {
            _tasks = TasksState.Value.OverdueList;
            _isLoading = TasksState.Value.IsOverdueListLoading;
            StateHasChanged();
        };

        Dispatcher.Dispatch(new LoadOverdueTasksListAction());
    }
    
    private Task OnAddTask(DateTime? endTime = null)
    {
        // return ModalDialogProviderService.ShowAddTaskModal(endTime: endTime);
        return Task.CompletedTask;
    }
}
