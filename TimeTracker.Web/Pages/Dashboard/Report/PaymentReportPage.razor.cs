using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Pages.Dashboard.Report;

public partial class PaymentReportPage
{
    [Inject] 
    private IState<ReportsState> _state { get; set; }

    public PaymentReportFilterState _filterState
    {
        get => _state.Value.PaymentReportFilter;
    }

    private IEnumerable<IGrouping<long?, PaymentsReportItemDto>> _grouppedItems
    {
        get => _state.Value.PaymentReportItems.GroupBy(item => item.ClientId);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }

    private void OnFooterCellRender(DataGridCellRenderEventArgs<PaymentsReportItemDto> args, PaymentsReportItemDto data)
    {
        if (args.Column.Property == "ProjectName")
        {
            var unpaidAmount = GetClientUnpaidAmount(data.ClientId ?? 0);
            args.Attributes.Add("style", $"background-color: {(unpaidAmount < 0 ? "var(--rz-danger)" : "var(--rz-success)")};");
            args.Attributes.Add("colspan", 1);
        }
    }

    private TimeSpan GetTotalDuration(long clientId)
    {
        var totalTicks = _state.Value.PaymentReportItems.Where(item => item.ClientId == clientId)
            .Sum(item => item.TotalDuration.Ticks);
        return new TimeSpan(totalTicks);
    }
    
    private decimal GetClientTotalAmount(long clientId)
    {
        return _state.Value.PaymentReportItems.Where(item => item.ClientId == clientId).Sum(item => item.Amount);
    }
    
    private decimal GetClientUnpaidAmount(long clientId)
    {
        var paidAmount = _state.Value.PaymentReportItems.FirstOrDefault(item => item.ClientId == clientId)?.PaidAmountByClient ?? 0;
        return paidAmount - GetClientTotalAmount(clientId);
    }

    private void OnChangeDateEnd(DateTime? endDate)
    {
        Dispatcher.Dispatch(new ReportSetPaymentReportFilterAction(new PaymentReportFilterState(EndDate: endDate.Value)));
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
}
