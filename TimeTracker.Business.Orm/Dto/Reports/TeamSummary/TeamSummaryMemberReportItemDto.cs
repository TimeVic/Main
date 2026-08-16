namespace TimeTracker.Business.Orm.Dto.Reports.TeamSummary;

public class TeamSummaryMemberReportItemDto
{
    public string? UserName { get; set; }

    public string Email { get; set; } = string.Empty;

    public object DurationAsEpoch { get; set; } = null!;

    public object ClientBillableOriginal { get; set; } = null!;

    public object TeamLaborCostOriginal { get; set; } = null!;

    public TimeSpan Duration => TimeSpan.FromSeconds(Math.Round(Convert.ToDouble(DurationAsEpoch)));

    public decimal ClientBillable => Convert.ToDecimal(ClientBillableOriginal);

    public decimal TeamLaborCost => Convert.ToDecimal(TeamLaborCostOriginal);
}
