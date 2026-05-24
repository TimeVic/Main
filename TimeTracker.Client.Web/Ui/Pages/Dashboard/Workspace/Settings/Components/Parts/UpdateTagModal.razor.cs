using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Client.Core.Store.Tag;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class UpdateTagModal
{
    [Parameter]
    public required TagDto Tag { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; } = false;

    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    public virtual IState<TagState> _state { get; set; }

    private UpdateRequest model = new();
    private EditForm _form;
    private LumexModal modal;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(Tag);
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        await OnCloseModal();
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
