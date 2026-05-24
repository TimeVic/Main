using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.ClientPayments.Parts.Modals;

public partial class ViewClientPaymentModal
{
    [Parameter]
    public required ClientPaymentDto ClientPayment { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
