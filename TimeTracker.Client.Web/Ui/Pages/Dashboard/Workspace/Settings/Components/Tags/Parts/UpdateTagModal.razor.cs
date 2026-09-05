using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Tag;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Tags.Parts;

public partial class UpdateTagModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required TagDto Tag { get; set; }

    [Inject]
    public virtual IState<TagState> _state { get; set; } = default!;

    private UpdateRequest model = new();
    private EditForm _form = default!;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(Tag);
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (Tag != null && model.TagId != Tag.Id)
        {
            model.Fill(Tag);
        }
        base.OnParametersSet();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }
}
