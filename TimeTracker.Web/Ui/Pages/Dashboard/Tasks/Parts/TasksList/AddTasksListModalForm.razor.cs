using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Project;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class AddTasksListModalForm
{
    public class Parameters
    {
        public Guid? ProjectId { get; set; }
    }
    
    [Parameter]
    public required Parameters Content { get; set; }
    
    [CascadingParameter] 
    FluentDialog MudDialog { get; set; }
    
    [Inject]
    public ILogger<AddTasksListModalForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.ProjectId = Content.ProjectId ?? Guid.Empty;
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
            var taskList = await ApiService.TaskListAddAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo("Task list has been added");
                OnCloseModal();
                
                NavigationManager.NavigateTo(
                    string.Format(
                        SiteUrl.Dashboard_Tasks,
                        taskList.Id
                    )    
                );
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
