using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.Summary.Parts;

public partial class TeamPerformanceTableSection
{
    [Parameter]
    public ICollection<TeamSummaryMemberReportItemDto> Items { get; set; } = new List<TeamSummaryMemberReportItemDto>();
}
