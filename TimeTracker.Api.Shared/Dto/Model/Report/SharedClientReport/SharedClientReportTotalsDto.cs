namespace TimeTracker.Api.Shared.Dto.Model.Report.SharedClientReport;

public class SharedClientReportTotalsDto
{
    public decimal Earned { get; set; }

    public decimal Received { get; set; }

    public decimal Outstanding => Earned - Received;
}
