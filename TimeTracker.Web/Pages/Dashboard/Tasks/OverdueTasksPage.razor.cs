using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class OverdueTasksPage
{
    [Inject]
    public IState<TasksState> TasksState { get; set; }

    [Inject]
    public ModalDialogProviderService ModalDialogProviderService { get; set; }
    
    private IEnumerable<TaskDto> _tasks => TasksState.Value.OverdueList;

    private DateTime _listStartDate = DateTime.Now.Date;
    private DateTime _listEndDate = DateTime.Now.Date.AddMonths(6);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        Dispatcher.Dispatch(new LoadOverdueTasksListAction());
    }
    
    private Task OnAddTask()
    {
        return ModalDialogProviderService.ShowAddTaskModal();
    }
}
