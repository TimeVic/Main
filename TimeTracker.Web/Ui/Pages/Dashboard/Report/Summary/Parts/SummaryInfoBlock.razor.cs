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
}
