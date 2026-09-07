using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Tag;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class AddTagModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Inject]
    public IState<TagState> _state { get; set; } = default!;

    private AddRequest model = new() { Name = string.Empty };
    private EditForm _form = default!;
    private bool _isLoading = false;

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddAction(model));
        model = new AddRequest { Name = string.Empty };
        if (ModalInstance != null)
        _isLoading = true;
        try
        {
            await ModalInstance.Close(AppModalResult.Ok());
            var response = await ApiService.TagAddAsync(model);
            if (response != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(response));
                ToastService.ShowSuccess(DashboardLocalizer["TagAdded"].Value);
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
