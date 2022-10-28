using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Model;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Orm.Dto.Reports;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<ProjectPaymentsReportItemDto, PaymentsReportItemDto>();
        CreateMap<ByDaysReportItemDto, SummaryByDaysReportItemDto>();
    }
}
