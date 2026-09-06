using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;

public partial class ViewClientPaymentModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required ClientPaymentDto ClientPayment { get; set; }

    private async Task OnCloseModal()
    {
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
    }
}
