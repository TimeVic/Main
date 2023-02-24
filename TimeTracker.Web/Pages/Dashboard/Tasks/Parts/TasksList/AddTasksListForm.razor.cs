using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Project;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class AddTasksListForm
{
    [Inject]
    public ILogger<AddTasksListForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;

    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            var taskList = await ApiService.TaskListAddAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Task list has been added"
                });
                DialogService.CloseSide();
                
                var navigateToProjectsClient = ProjectState.Value.List.FirstOrDefault(
                    item => item.Id == model.ProjectId
                );
                NavigationManager.NavigateTo(
                    string.Format(
                        SiteUrl.Dashboard_Tasks,
                        navigateToProjectsClient?.Client?.Id.ToString() ?? "0",
                        taskList.Id
                    )    
                );
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
