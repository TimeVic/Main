using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface ISummaryReportDao: IDomainService
{
    Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByDaysReportItemDto>> GetReportByDayForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );

    Task<ICollection<ByProjectsReportItemDto>> GetReportByProjectForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByProjectsReportItemDto>> GetReportByProjectForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );

    Task<ICollection<ByClientsReportItemDto>> GetReportByClientForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByClientsReportItemDto>> GetReportByClientForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );

    Task<ICollection<ByUsersReportItemDto>> GetReportByUserForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByUsersReportItemDto>> GetReportByUserForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );

    Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );

    Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    );

    Task<ICollection<ByWeeksReportItemDto>> GetReportByWeekForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );
}
