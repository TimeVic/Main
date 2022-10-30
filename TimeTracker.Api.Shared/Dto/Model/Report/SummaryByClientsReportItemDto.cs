namespace TimeTracker.Api.Shared.Dto.Model.Report;

public class SummaryByClientsReportItemDto
{
    public long? ClientId { get; set; }

    public TimeSpan Duration { get; set; }
}
