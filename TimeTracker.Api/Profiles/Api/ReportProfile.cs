using AutoMapper;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Reports;
using TimeTracker.Business.Orm.Dto.Reports.Summary;

namespace TimeTracker.Api.Profiles.Api;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<ProjectMemberPaymentsReportItemDto, MemberPaymentsReportItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new MemberPaymentsReportItemDto
            {
                ProjectId = src.ProjectId,
                ProjectName = src.ProjectName,
                ClientId = src.ClientId,
                ClientName = src.ClientName,
                Amount = src.Amount,
                PaidAmountByClient = src.PaidAmountByClient,
                PaidAmountByProject = src.PaidAmountByProject,
                TotalDuration = src.TotalDuration
            });
        CreateMap<ByDaysReportItemDto, SummaryByDaysReportItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new SummaryByDaysReportItemDto
            {
                Date = src.Date,
                Duration = src.Duration,
                Amount = src.Amount
            });
        CreateMap<ByMonthsReportItemDto, SummaryByMonthsReportItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new SummaryByMonthsReportItemDto
            {
                Month = src.Month,
                Year = src.Year,
                Duration = src.Duration,
                Amount = src.Amount
            });
        CreateMap<ByWeeksReportItemDto, SummaryByWeeksReportItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new SummaryByWeeksReportItemDto
            {
                WeekStartDate = src.WeekStartDate,
                WeekEndDate = src.WeekEndDate,
                Duration = src.Duration,
                Amount = src.Amount
            });
        CreateMap<ByClientsReportItemDto, SummaryByClientsReportItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new SummaryByClientsReportItemDto
            {
                ClientId = src.ClientId,
                ClientName = src.ClientName,
                Duration = src.Duration,
                Amount = src.Amount
            });
        CreateMap<ByProjectsReportItemDto, SummaryByProjectsReportItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new SummaryByProjectsReportItemDto
            {
                ProjectId = src.ProjectId,
                ProjectName = src.ProjectName,
                Duration = src.Duration,
                Amount = src.Amount
            });
    }
}
