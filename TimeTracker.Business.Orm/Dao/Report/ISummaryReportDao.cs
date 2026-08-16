using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.Reports.Summary;

using TimeTracker.Business.Orm.Dto.Reports.TeamSummary;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface ISummaryReportDao: IDomainService
{
    Task<ICollection<ByDaysReportItemDto>> GetReportByDayAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByProjectsReportItemDto>> GetReportByProjectAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByClientsReportItemDto>> GetReportByClientAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<TeamSummaryByDaysReportItemDto>> GetTeamReportByDayAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<TeamSummaryMemberReportItemDto>> GetTeamReportByMemberAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    );
}
