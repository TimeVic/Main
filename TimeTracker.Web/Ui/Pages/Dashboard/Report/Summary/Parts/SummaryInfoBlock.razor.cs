using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.Summary.Parts;

public partial class SummaryInfoBlock
{
    [Inject]
    public IState<ReportsState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    public IEnumerable<SummaryByDaysReportItemDto> _byDateItems
    {
        get => _state.Value.SummaryReportData?.ByDays ?? new List<SummaryByDaysReportItemDto>();
    }
    
    public TimeSpan _totalDuration
    {
        get => new(_byDateItems.Sum(item => item.Duration.Ticks));
    }
    public decimal _totalAmount
    {
        get => _byDateItems.Sum(item => item.Amount);
    }

    private IEnumerable<IGrouping<Guid?, PaymentsReportItemDto>> _paymentReportGroups
    {
        get => _state.Value.PaymentReportItems.GroupBy(item => item.ClientId);
    }

    public decimal _paymentReportEarned
    {
        get => _state.Value.PaymentReportItems.Sum(item => item.Amount);
    }

    public decimal _totalPaid
    {
        get => _paymentReportGroups.Sum(group => group.FirstOrDefault()?.PaidAmountByClient ?? 0);
    }

    public decimal _outstandingBalance
    {
        get => _paymentReportGroups.Sum(group =>
        {
            var earned = group.Sum(item => item.Amount);
            var paid = group.FirstOrDefault()?.PaidAmountByClient ?? 0;
            return Math.Max(earned - paid, 0);
        });
    }
}
