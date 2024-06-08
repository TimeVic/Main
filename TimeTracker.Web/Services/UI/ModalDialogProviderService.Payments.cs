using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Payment.Parts;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddPaymentModal()
    {
        await _mudDialogService.ShowAsync<AddPaymentModal>("Add new payment");
    }
    
    public async Task ShowUpdatePaymentModal(PaymentDto payment)
    {
        var parameters = new DialogParameters<UpdatePaymentModal>
        {
            { x => x.Payment, payment },
        };
        await _mudDialogService.ShowAsync<UpdatePaymentModal>("Update payment", parameters);
    }
}
