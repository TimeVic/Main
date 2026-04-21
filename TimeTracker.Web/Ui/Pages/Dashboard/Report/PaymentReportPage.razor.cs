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

    private IEnumerable<IGrouping<Guid?, PaymentsReportItemDto>> _groupedItems
    {
        get => _state.Value.PaymentReportItems.GroupBy(item => item.ClientId);
    }

    private IEnumerable<IGrouping<Guid?, PaymentsReportItemDto>> _sortedGroupedItems
    {
        get => _groupedItems
            .OrderBy(GetClientSortBucket)
            .ThenByDescending(GetClientOutstandingAmount)
            .ThenByDescending(GetClientTotalAmount)
            .ThenBy(GetClientDisplayName);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
    
    private static decimal GetClientTotalAmount(IGrouping<Guid?, PaymentsReportItemDto> group)
    {
        return group.Sum(item => item.Amount);
    }

    private static decimal GetClientReceivedAmount(IGrouping<Guid?, PaymentsReportItemDto> group)
    {
        return group.FirstOrDefault()?.PaidAmountByClient ?? 0;
    }

    private static decimal GetClientOutstandingAmount(IGrouping<Guid?, PaymentsReportItemDto> group)
    {
        return Math.Max(GetClientTotalAmount(group) - GetClientReceivedAmount(group), 0);
    }

    private static int GetClientSortBucket(IGrouping<Guid?, PaymentsReportItemDto> group)
    {
        var earned = GetClientTotalAmount(group);
        var received = GetClientReceivedAmount(group);
        var outstanding = GetClientOutstandingAmount(group);

        if (outstanding > 0)
        {
            return 0;
        }

        if (earned > 0 || received > 0)
        {
            return 1;
        }

        return 2;
    }

    private static string GetClientDisplayName(IGrouping<Guid?, PaymentsReportItemDto> group)
    {
        var clientName = group.FirstOrDefault()?.ClientName;
        return string.IsNullOrWhiteSpace(clientName) ? "Other projects" : clientName;
    }

    private void OnChangeDateEnd(DateTime? endDate)
    {
        Dispatcher.Dispatch(new ReportSetPaymentReportFilterAction(new PaymentReportFilterState(EndDate: endDate.Value)));
        Dispatcher.Dispatch(new ReportFetchPaymentsReportAction());
    }
}
