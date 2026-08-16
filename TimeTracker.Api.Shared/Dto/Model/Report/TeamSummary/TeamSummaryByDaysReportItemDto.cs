namespace TimeTracker.Api.Shared.Dto.Model.Report.TeamSummary;

public class TeamSummaryByDaysReportItemDto
{
    public DateTime Date { get; set; }

    public TimeSpan Duration { get; set; }

    public decimal ClientBillable { get; set; }

    public decimal TeamLaborCost { get; set; }

    public decimal GrossProfit => ClientBillable - TeamLaborCost;
}
