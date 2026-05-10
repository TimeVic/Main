using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report;

public partial class MemberPaymentReportPage
{
    [Inject] 
    private IState<ReportsState> _state { get; set; }

    public MemberPaymentReportFilterState _filterState
    {
        get => _state.Value.MemberPaymentReportFilter;
    }

    private IEnumerable<IGrouping<Guid?, MemberPaymentsReportItemDto>> _groupedItems
    {
        get => _state.Value.MemberPaymentReportItems.GroupBy(item => item.ClientId);
    }

    private IEnumerable<IGrouping<Guid?, MemberPaymentsReportItemDto>> _sortedGroupedItems
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
        Dispatcher.Dispatch(new ReportFetchMemberPaymentsReportAction());
    }
    
    private static decimal GetClientTotalAmount(IGrouping<Guid?, MemberPaymentsReportItemDto> group)
    {
        return group.Sum(item => item.Amount);
    }

    private static decimal GetClientReceivedAmount(IGrouping<Guid?, MemberPaymentsReportItemDto> group)
    {
        return group.FirstOrDefault()?.PaidAmountByClient ?? 0;
    }

    private static decimal GetClientOutstandingAmount(IGrouping<Guid?, MemberPaymentsReportItemDto> group)
    {
        return Math.Max(GetClientTotalAmount(group) - GetClientReceivedAmount(group), 0);
    }

    private static int GetClientSortBucket(IGrouping<Guid?, MemberPaymentsReportItemDto> group)
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

    private string GetClientDisplayName(IGrouping<Guid?, MemberPaymentsReportItemDto> group)
    {
        var clientName = group.FirstOrDefault()?.ClientName;
        return string.IsNullOrWhiteSpace(clientName) ? DashboardLocalizer["MemberPaymentReportPage_OtherProjects"].Value : clientName;
    }

    private void OnChangeDateEnd(DateTime? endDate)
    {
        Dispatcher.Dispatch(new ReportSetMemberPaymentReportFilterAction(new MemberPaymentReportFilterState(EndDate: endDate.Value)));
        Dispatcher.Dispatch(new ReportFetchMemberPaymentsReportAction());
    }
}
