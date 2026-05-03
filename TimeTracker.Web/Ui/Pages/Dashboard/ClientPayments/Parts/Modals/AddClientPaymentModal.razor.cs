using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment;
using TimeTracker.Web.Services.DateTimes;
using TimeTracker.Web.Store.ClientPayments;

namespace TimeTracker.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;

public partial class AddClientPaymentModal
{
    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    private AddRequest model = new();
    private EditForm _form;
    private LumexModal modal;

    protected override async Task OnInitializedAsync()
    {
        InitModel();
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        model.WorkspaceId = AuthState.Value.Workspace!.Id;
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new AddClientPaymentAction(model));
        InitModel();
        await modal.CloseAsync();
        StateHasChanged();
    }

    private void OnCloseModal()
    {
        IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
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
