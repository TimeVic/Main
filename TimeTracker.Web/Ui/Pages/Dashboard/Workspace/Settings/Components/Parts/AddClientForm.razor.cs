using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class AddClientForm
{
    [Inject]
    public ILogger<AddClientForm> _logger { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;
    
    private async Task Submit()
    {
        model.WorkspaceId = AuthState.Value.Workspace!.Id;
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        
        _isLoading = true;
        try
        {
            var workspace = await ApiService.ClientAddAsync(model);
            if (workspace != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo("Client has been added");
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
