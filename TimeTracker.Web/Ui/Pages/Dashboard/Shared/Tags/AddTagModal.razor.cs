using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tags;

public partial class AddTagModal
{
    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    public IState<TagState> _state { get; set; }

    private AddRequest model = new() { Name = string.Empty };
    private EditForm _form;
    private LumexModal modal;

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddAction(model));
        await OnCloseModal();
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        model = new AddRequest { Name = string.Empty };
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
