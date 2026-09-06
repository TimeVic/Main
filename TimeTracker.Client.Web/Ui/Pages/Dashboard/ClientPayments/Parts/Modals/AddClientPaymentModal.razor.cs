using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.ClientPayments;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;

public partial class AddClientPaymentModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    private AddRequest model = new();
    private EditForm _form = default!;

    protected override async Task OnInitializedAsync()
    {
        InitModel();
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddClientPaymentAction(model));
        InitModel();
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();
    }

    private void InitModel()
    {
        model = new AddRequest
        {
            PaymentTime = UserDateTimeProviderService.GetCurrentTime().DateTime
        };
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
