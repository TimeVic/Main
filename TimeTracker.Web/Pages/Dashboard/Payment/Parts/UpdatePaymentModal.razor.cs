using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Payment;
using TimeTracker.Web.Store.Payment;

namespace TimeTracker.Web.Pages.Dashboard.Payment.Parts;

public partial class UpdatePaymentModal
{
    public class Parameters
    {
        public required PaymentDto Payment { get; set; }
    }
    
    [Parameter]
    public required Parameters Content { get; set; }
    
    [CascadingParameter] 
    public FluentDialog MudDialog { get; set; }

    private UpdateRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private EditForm _form;

    private long _projectId
    {
        get => model.ProjectId;
        set => model.ProjectId = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Content.Payment);
        model.WorkspaceId = AuthState.Value.Workspace.Id;
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        _isLoading = true;
        try
        {
            var responseDto = await ApiService.PaymentUpdateAsync(model);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                ToastService.ShowInfo("Payment updated");
                OnCloseModal();
            }
        }
        catch (Exception e)
        {
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();    
    }

    private void OnCloseModal()
    {
        MudDialog.Hide();
    }

    private void OnUpdateTime(DateTime? paymentTime)
    {
        if (paymentTime.HasValue)
        {
            model.PaymentTime = paymentTime.Value;
        }
    }
}
