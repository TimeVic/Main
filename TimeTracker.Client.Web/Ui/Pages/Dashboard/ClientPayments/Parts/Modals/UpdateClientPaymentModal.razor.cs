using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.ClientPayments;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;

public partial class UpdateClientPaymentModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required ClientPaymentDto ClientPayment { get; set; }

    [Inject]
    public IState<ClientPaymentState> _state { get; set; } = default!;

    private UpdateRequest model = new();
    private EditForm _form = default!;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(ClientPayment);
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (ClientPayment != null && model.ClientPaymentId != ClientPayment.Id)
        {
            model.Fill(ClientPayment);
        }
        base.OnParametersSet();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateClientPaymentAction(model));
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
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
