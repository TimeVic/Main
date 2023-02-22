using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class UpdateTasksListForm
{
    [Parameter]
    public TaskListDto TaskList { get; set; }
    
    [Inject]
    public ILogger<AddTasksListForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private UpdateRequest model = new();
    private bool _isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.ProjectId = TaskList.Project.Id;
        model.TaskListId = TaskList.Id;
        model.Name = TaskList.Name;
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            var taskList = await ApiService.TaskListUpdateAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Task list has been updated"
                });
                DialogService.CloseSide();
                Dispatcher.Dispatch(new SetListItemAction(taskList));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task list adding error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
    
    private void CloseModal()
    {
        DialogService.CloseSide();
    }
}
