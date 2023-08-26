using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Web.Store.Client;

namespace TimeTracker.Web.Pages.Dashboard.Client.Parts.List;

public partial class AddClientModal
{
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }

    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
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
            model.WorkspaceId = AuthState.Value.Workspace.Id;
            var responseDto = await ApiService.ClientAddAsync(model);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                await ToastService.ShowInfo("Client added");
                OnCloseModal();
            }
        }
        catch (Exception e)
        {
            await ToastService.ShowError(e.Message);
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
