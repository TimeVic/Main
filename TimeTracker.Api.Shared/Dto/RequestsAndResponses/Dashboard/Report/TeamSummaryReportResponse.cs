using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class TeamSummaryReportResponse : IResponse
{
    public TeamSummaryTotalsDto Totals { get; set; } = new();

    public ICollection<TeamSummaryByDaysReportItemDto> ByDays { get; set; } = new List<TeamSummaryByDaysReportItemDto>();

    public ICollection<TeamSummaryMemberReportItemDto> Members { get; set; } = new List<TeamSummaryMemberReportItemDto>();
}
