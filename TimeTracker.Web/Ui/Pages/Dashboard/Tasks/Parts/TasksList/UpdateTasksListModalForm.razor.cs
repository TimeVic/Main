using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Store.Project;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;
using SetListItemAction = TimeTracker.Web.Store.TasksList.SetListItemAction;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class UpdateTasksListModalForm
{
    public class Parameters
    {
        public TaskListDto TaskList { get; set; }
    }
    
    [Parameter]
    public required Parameters Content { get; set; }

    [CascadingParameter] 
    FluentDialog MudDialog { get; set; }
    
    [Inject]
    public ILogger<UpdateTasksListModalForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private UpdateRequest model = new();
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.ProjectId = Content.TaskList.Project.Id;
        model.TaskListId = Content.TaskList.Id;
        model.Name = Content.TaskList.Name;
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
            var taskList = await ApiService.TaskListUpdateAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo("Task list has been updated");
                OnCloseModal();
                Dispatcher.Dispatch(new SetListItemAction(taskList));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            ToastService.ShowError("Task list adding error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
    
    private void OnCloseModal()
    {
        MudDialog.CloseAsync();
    }
}
