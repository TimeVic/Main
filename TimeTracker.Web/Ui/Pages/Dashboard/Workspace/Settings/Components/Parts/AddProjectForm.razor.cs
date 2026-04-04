using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class AddProjectForm
{
    [Inject]
    public ILogger<AddProjectForm> _logger { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;
    
    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        
        _isLoading = true;
        try
        {
            model.WorkspaceId = AuthState.Value.Workspace!.Id;
            var workspace = await ApiService.ProjectAddAsync(model);
            if (workspace != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo("Project has been added");
                model = new AddRequest();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            ToastService.ShowError("Client adding error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
