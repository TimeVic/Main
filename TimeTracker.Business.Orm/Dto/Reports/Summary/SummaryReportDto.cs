namespace TimeTracker.Business.Orm.Dto.Reports.Summary;

public class SummaryReportDto
{
    public ICollection<ByDaysReportItemDto> ByDays { get; set; }
}
