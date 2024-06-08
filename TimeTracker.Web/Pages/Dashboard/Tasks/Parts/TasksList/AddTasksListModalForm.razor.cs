using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Project;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class AddTasksListModalForm
{
    [Parameter]
    public long? ProjectId { get; set; }
    
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public ILogger<AddTasksListModalForm> _logger { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private MudForm _form;
    private bool _isValid = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.ProjectId = ProjectId ?? 0;
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
            var taskList = await ApiService.TaskListAddAsync(model);
            if (taskList != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                await ToastService.ShowInfo("Task list has been added");
                OnCloseModal();
                
                var navigateToProject = ProjectState.Value.List.First(
                    item => item.Id == model.ProjectId
                );
                NavigationManager.NavigateTo(
                    string.Format(
                        SiteUrl.Dashboard_Tasks,
                        navigateToProject?.Id.ToString(),
                        taskList.Id
                    )    
                );
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
