using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Web.Store.ClientPayments;

namespace TimeTracker.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;

public partial class UpdateClientPaymentModal
{
    [Parameter]
    public required ClientPaymentDto ClientPayment { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    public IState<ClientPaymentState> _state { get; set; }

    private UpdateRequest model = new();
    private EditForm _form;
    private LumexModal modal;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(ClientPayment);
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        model.WorkspaceId = AuthState.Value.Workspace!.Id;
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateClientPaymentAction(model));
        await OnCloseModal();
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }

    private void OnChangeClient(ClientDto? client)
    {
        model.ClientId = client?.Id ?? Guid.Empty;
        model.ProjectId = Guid.Empty;
    }

    private void OnProjectSelected(ProjectDto? project)
    {
        model.ProjectId = project?.Id ?? Guid.Empty;
    }
}
