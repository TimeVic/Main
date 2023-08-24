using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class UpdateTasksListModalForm
{
    [Parameter]
    public TaskListDto TaskList { get; set; }
    
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public ILogger<UpdateTasksListModalForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private UpdateRequest model = new();
    private bool _isLoading = false;
    private MudForm _form;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.ProjectId = TaskList.Project.Id;
        model.TaskListId = TaskList.Id;
        model.Name = TaskList.Name;
    }
    
    private async Task Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }
        
        _isLoading = true;
        try
        {
            var taskList = await ApiService.TaskListUpdateAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                await ToastService.ShowInfo("Task list has been updated");
                OnCloseModal();
                Dispatcher.Dispatch(new SetListItemAction(taskList));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            await ToastService.ShowError("Task list adding error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
    
    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
