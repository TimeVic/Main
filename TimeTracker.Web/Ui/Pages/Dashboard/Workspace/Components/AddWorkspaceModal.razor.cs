using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Components;

public partial class AddWorkspaceModal
{
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback<WorkspaceDto?> OnAdded { get; set; }
    
    [Inject]
    public ILogger<AddWorkspaceModal> _logger { get; set; }
    
    [Inject]
    public WorkspaceInitializationService WorkspaceInitializationService { get; set; }
    
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
            var workspace = await ApiService.WorkspaceAddAsync(model.Name);
            if (workspace != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo(DashboardLocalizer["AddWorkspaceModal_WorkspaceCreated"].Value);
                IsOpened = false;
                model = new AddRequest();
                await OnAdded.InvokeAsync(workspace);
                await modal.CloseAsync();
                WorkspaceInitializationService.ChangeWorkspace(workspace);
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

    private void OnCloseModal()
    {
        IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
