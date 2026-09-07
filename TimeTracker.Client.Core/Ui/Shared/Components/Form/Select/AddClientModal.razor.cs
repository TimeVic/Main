using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Client;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class AddClientModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Inject]
    public IState<ClientState> _state { get; set; } = default!;

    private AddRequest model = new();
    private EditForm _form = default!;
    private bool _isLoading = false;

    private Task SubmitForm(EditContext editContext)
    {
        return Submit();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddAction(model));
        model = new AddRequest();
        if (ModalInstance != null)
        _isLoading = true;
        try
        {
            await ModalInstance.Close(AppModalResult.Ok());
            var response = await ApiService.ClientAddAsync(model);
            if (response != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(response));
                ToastService.ShowSuccess(DashboardLocalizer["ClientAdded"].Value);
                if (ModalInstance != null)
                {
                    await ModalInstance.Close(AppModalResult.Ok(response));
                }
            }
        }
        StateHasChanged();
        catch (Exception e)
        {
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}
