using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Web.Store.Report;

namespace TimeTracker.Web.Pages.Dashboard.Report.Summary.Parts;

public partial class SummaryInfoBlock
{
    [Inject]
    public IState<ReportsState> _state { get; set; }
    
    public IEnumerable<SummaryByDaysReportItemDto> _byDateItems
    {
        get => _state.Value.SummaryReportData?.ByDays ?? new List<SummaryByDaysReportItemDto>();
    }
    
    public TimeSpan _totalDuration
    {
        get => new(_byDateItems.Sum(item => item.Duration.Ticks));
    }
}
