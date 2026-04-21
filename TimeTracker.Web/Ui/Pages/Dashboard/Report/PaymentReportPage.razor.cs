using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report;

public partial class PaymentReportPage
{
    [Inject] 
    private IState<ReportsState> _state { get; set; }

    public PaymentReportFilterState _filterState
    {
        get => _state.Value.PaymentReportFilter;
    }

    private IEnumerable<IGrouping<Guid?, PaymentsReportItemDto>> _grouppedItems
    {
        get => _state.Value.PaymentReportItems.GroupBy(item => item.ClientId);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
    
    private TimeSpan GetTotalDuration(Guid? clientId)
    {
        var totalTicks = _state.Value.PaymentReportItems.Where(item => item.ClientId == clientId)
            .Sum(item => item.TotalDuration.Ticks);
        return new TimeSpan(totalTicks);
    }
    
    private decimal GetClientTotalAmount(Guid? clientId)
    {
        return _state.Value.PaymentReportItems.Where(item => item.ClientId == clientId).Sum(item => item.Amount);
    }
    
    private decimal GetClientOutstandingAmount(Guid? clientId)
    {
        var paidAmount = _state.Value.PaymentReportItems.FirstOrDefault(item => item.ClientId == clientId)?.PaidAmountByClient ?? 0;
        return Math.Max(GetClientTotalAmount(clientId) - paidAmount, 0);
    }

    private static decimal GetProjectOutstandingAmount(PaymentsReportItemDto item)
    {
        return Math.Max(item.Amount - item.PaidAmountByProject, 0);
    }

    private static decimal GetEffectiveHourlyRate(decimal amount, TimeSpan duration)
    {
        if (duration.TotalHours <= 0)
        {
            return 0;
        }

        return Math.Round(amount / (decimal)duration.TotalHours, 2);
    }

    private void OnChangeDateEnd(DateTime? endDate)
    {
        Dispatcher.Dispatch(new ReportSetPaymentReportFilterAction(new PaymentReportFilterState(EndDate: endDate.Value)));
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
}
