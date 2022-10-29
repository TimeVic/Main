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
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    );
}
