using LumexUI;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Projects;

public partial class AddProjectModal
{
    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public Guid? InitialClientId { get; set; }

    [Inject]
    public IState<ProjectState> _state { get; set; }

    private AddRequest model = new();
    private EditForm _form;
    private LumexModal modal;
    
    protected override void OnParametersSet()
    {
        if (IsOpened && InitialClientId.HasValue && model.ClientId == Guid.Empty)
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
        await OnCloseModal();
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        model = new AddRequest();
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }

    private void OnClientChanged(ClientDto? client)
    {
        model.ClientId = client?.Id ?? Guid.Empty;
    }
}
