using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Project;
using LoadListAction = TimeTracker.Client.Core.Store.TasksList.LoadListAction;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class AddTasksListModalForm
{   
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public ProjectDto? Project { get; set; }
    
    [Parameter]
    public virtual EventCallback<TaskListDto?> OnAdded { get; set; }
    
    [Inject]
    public ILogger<AddTasksListModalForm> _logger { get; set; } = default!;
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; } = default!;
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private EditForm _form = default!;

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        
        _isLoading = true;
        try
        {
            model.ProjectId = Project?.Id ?? Guid.Empty;
            var taskList = await ApiService.TaskListAddAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetListItemAction(taskList));
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo(DashboardLocalizer["AddTasksListModalForm_TaskListAdded"].Value);
                model = new AddRequest();
                await OnAdded.InvokeAsync(taskList);
                if (ModalInstance != null)
                {
                    await ModalInstance.Close(AppModalResult.Ok(taskList));
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            ToastService.ShowError(DashboardLocalizer["AddTasksListModalForm_TaskListAddingError"].Value);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
