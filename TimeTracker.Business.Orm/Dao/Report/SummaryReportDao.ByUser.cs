using NHibernate;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: ISummaryReportDao
{
    private const string SqlQuerySummaryByUserForOwner = @"
        select
            te.user_id as UserId,
            u.user_name as UserName,
            u.email as Email,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
            sum(
	            round(
	                te.hourly_rate / 60 / 60 -- Price per second 
	                *
	                extract(epoch from te.end_time - te.start_time), -- Total seconds
	                2
	            )
            ) as AmountOriginal
        from time_entries te 
        inner join users u on u.id = te.user_id
        where te.workspace_id = :workspaceId and te.date >= :startDate and te.date <= :endDate
        group by te.user_id, u.user_name, u.email
    ";
    
    public async Task<ICollection<ByUsersReportItemDto>> GetReportByUserForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByUsersReportItemDto>(
            SqlQuerySummaryByUserForOwner,
            workspaceId,
            startDate,
            endDate
        );
    }
    
    private const string SqlQuerySummaryByUserForOther = @"
        select
            te.user_id as UserId,
            u.user_name as UserName,
            u.email as Email,
            sum(extract(epoch from te.end_time - te.start_time)) as DurationAsEpoch,
            sum(
                case when te.user_id = :userId
                    then round(
	                    te.hourly_rate / 60 / 60 -- Price per second 
	                    *
	                    extract(epoch from te.end_time - te.start_time), -- Total seconds
	                    2
	                )
                    else 0
                end
            ) as AmountOriginal
        from time_entries te 
        inner join users u on u.id = te.user_id
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by te.user_id, u.user_name, u.email
    ";
    
    public async Task<ICollection<ByUsersReportItemDto>> GetReportByUserForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByUsersReportItemDto>(
            SqlQuerySummaryByUserForOther,
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
}
