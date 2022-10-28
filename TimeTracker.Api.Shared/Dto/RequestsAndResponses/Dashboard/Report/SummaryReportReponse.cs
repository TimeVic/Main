using TimeTracker.Api.Shared.Dto.Model.Report;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class SummaryReportDto
{
    public ICollection<SummaryByDaysReportItemDto> ByDays { get; set; }
}
