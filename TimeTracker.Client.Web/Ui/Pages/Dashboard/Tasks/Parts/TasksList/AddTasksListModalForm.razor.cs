using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Store.Project;
using LoadListAction = TimeTracker.Client.Core.Store.TasksList.LoadListAction;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class AddTasksListModalForm
{   
    [Parameter]
    public required ProjectDto? Project { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback<TaskListDto?> OnAdded { get; set; }
    
    [Inject]
    public ILogger<AddTasksListModalForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;
    private LumexModal modal;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

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
                Dispatcher.Dispatch(new LoadListAction(true, Project?.Id));
                ToastService.ShowInfo(DashboardLocalizer["AddTasksListModalForm_TaskListAdded"].Value);
                IsOpened = false;
                model = new AddRequest();
                await OnAdded.InvokeAsync(taskList);
                await modal.CloseAsync();
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

    private void OnCloseModal()
    {
        IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
