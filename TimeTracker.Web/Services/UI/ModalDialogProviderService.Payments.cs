using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Payment.Parts;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowAddPaymentModal()
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            TrapFocus = false
        };
        await _dialogService.ShowDialogAsync<AddPaymentModal>(parameters);
    }
    
    public async Task ShowUpdatePaymentModal(PaymentDto payment)
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            TrapFocus = false,
        };
        await _dialogService.ShowDialogAsync<UpdatePaymentModal>(
            new UpdatePaymentModal.Parameters()
            {
                Payment = payment,
            },
            parameters
        );
    }
}
