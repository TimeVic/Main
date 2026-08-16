namespace TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;

public class TeamSummaryMemberReportItemDto
{
    public string? UserName { get; set; }

    public string Email { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }

    public decimal TeamLaborCost { get; set; }

    public decimal ClientBillable { get; set; }

    public decimal GrossProfit => ClientBillable - TeamLaborCost;

    public decimal? MarginPercent => ClientBillable == 0
        ? null
        : Math.Round(GrossProfit / ClientBillable * 100, 1);

    public string Name => string.IsNullOrWhiteSpace(UserName) ? Email : UserName;
}
