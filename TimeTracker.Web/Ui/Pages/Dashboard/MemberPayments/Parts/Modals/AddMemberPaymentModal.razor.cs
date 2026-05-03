using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment;
using TimeTracker.Web.Services.DateTimes;
using TimeTracker.Web.Store.MemberPayments;

namespace TimeTracker.Web.Ui.Pages.Dashboard.MemberPayments.Parts.Modals;

public partial class AddMemberPaymentModal
{   
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Inject]
    public ILogger<AddMemberPaymentModal> _logger { get; set; }
    
    private AddRequest model;
    private bool _isLoading = false;
    private EditForm _form;
    private bool _isValid = false;
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
        
        Dispatcher.Dispatch(new AddMemberPaymentAction(model));
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
        model = new AddRequest();
        model.PaymentTime = UserDateTimeProviderService.GetCurrentTime().DateTime;
    }

    private void OnChangeClient(ClientDto client)
    {
        model.ClientId = client.Id;
        model.ProjectId = Guid.Empty;
    }

    private void OnProjectSelected(ProjectDto project)
    {
        model.ProjectId = project.Id;
    }
}
