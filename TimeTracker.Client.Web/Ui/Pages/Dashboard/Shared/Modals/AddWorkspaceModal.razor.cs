using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Web.Services.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;

public partial class AddWorkspaceModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public virtual EventCallback<WorkspaceDto?> OnAdded { get; set; }
    
    [Inject]
    public ILogger<AddWorkspaceModal> _logger { get; set; } = default!;
    
    [Inject]
    public WorkspaceInitializationService WorkspaceInitializationService { get; set; } = default!;
    
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
            var workspace = await ApiService.WorkspaceAddAsync(model.Name);
            if (workspace != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo(DashboardLocalizer["AddWorkspaceModal_WorkspaceCreated"].Value);
                model = new AddRequest();
                await OnAdded.InvokeAsync(workspace);
                if (ModalInstance != null)
                {
                    await ModalInstance.Close(AppModalResult.Ok(workspace));
                }
                WorkspaceInitializationService.ChangeWorkspace(workspace, "workspace/choose-mode");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            ToastService.ShowError(DashboardLocalizer["AddWorkspaceModal_WorkspaceAddingError"].Value);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
