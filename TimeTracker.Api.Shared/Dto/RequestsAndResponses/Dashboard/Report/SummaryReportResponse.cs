using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Model.Report;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class SummaryReportResponse: IResponse
{
    public ICollection<SummaryByDaysReportItemDto> ByDays { get; set; } = new List<SummaryByDaysReportItemDto>();
    
    public ICollection<SummaryByDaysReportItemDto>? GroupedByDay { get; set; }
    
    public ICollection<SummaryByClientsReportItemDto>? GroupedByClient { get; set; }
    
    public ICollection<SummaryByProjectsReportItemDto>? GroupedByProject { get; set; }
    
    public ICollection<SummaryByMonthsReportItemDto>? GroupedByMonth { get; set; }
    
    public ICollection<SummaryByWeeksReportItemDto>? GroupedByWeek { get; set; }
    
    public ICollection<SummaryByUsersReportItemDto>? GroupedByUser { get; set; }
}

