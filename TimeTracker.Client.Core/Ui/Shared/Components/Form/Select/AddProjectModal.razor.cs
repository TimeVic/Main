using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class AddProjectModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }
    
    [Parameter]
    public Guid? InitialClientId { get; set; }

    [Inject]
    public IState<ProjectState> _state { get; set; } = default!;

    private AddRequest model = new();
    private EditForm _form = default!;
    
    protected override void OnParametersSet()
    {
        if (InitialClientId.HasValue && model.ClientId == Guid.Empty)
        {
            model.ClientId = InitialClientId.Value;
        }

        base.OnParametersSet();
    }

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
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }

    private void OnClientChanged(ClientDto? client)
    {
        model.ClientId = client?.Id ?? Guid.Empty;
    }
}
