using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Tag;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tags;

public partial class AddTagModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Inject]
    public IState<TagState> _state { get; set; } = default!;

    private AddRequest model = new() { Name = string.Empty };
    private EditForm _form = default!;

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddAction(model));
        model = new AddRequest { Name = string.Empty };
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }
}
