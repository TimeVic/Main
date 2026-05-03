using AutoMapper;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Orm.Dto.Reports;
using TimeTracker.Business.Orm.Dto.Reports.Summary;

namespace TimeTracker.Api.Profiles.Api;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<ProjectMemberPaymentsReportItemDto, MemberPaymentsReportItemDto>();
        CreateMap<ByDaysReportItemDto, SummaryByDaysReportItemDto>();
        CreateMap<ByMonthsReportItemDto, SummaryByMonthsReportItemDto>();
        CreateMap<ByWeeksReportItemDto, SummaryByWeeksReportItemDto>();
        CreateMap<ByClientsReportItemDto, SummaryByClientsReportItemDto>();
        CreateMap<ByProjectsReportItemDto, SummaryByProjectsReportItemDto>();
        CreateMap<ByUsersReportItemDto, SummaryByUsersReportItemDto>();
    }
}
