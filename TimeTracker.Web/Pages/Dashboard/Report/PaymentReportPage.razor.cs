using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Pages.Dashboard.Report;

public partial class PaymentReportPage
{
    [Inject] 
    private IState<ReportsState> _state { get; set; }

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
        Debug.Log(args.Column.Property);
        if (args.Column.Property == "ProjectName")
        {
            args.Attributes.Add("style", $"background-color: {(data.UnpaidAmount < 0 ? "var(--rz-danger)" : "var(--rz-success)")};");
            args.Attributes.Add("colspan", 2);
        }
    }
}
