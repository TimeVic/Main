﻿using NHibernate;
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
    private const string SqlQuerySummaryByClientForOwner = @"
        select
            c.id as ClientId,
            c.name as ClientName,
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
        left join projects p on p.id = te.project_id
        left join clients c on c.id = p.client_id 
        where te.workspace_id = :workspaceId and te.date >= :startDate and te.date <= :endDate
        group by c.id, c.name
    ";
    
    public async Task<ICollection<ByClientsReportItemDto>> GetReportByClientForOwnerOrManagerAsync(
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportForOwnerOrManagerAsync<ByClientsReportItemDto>(
            SqlQuerySummaryByClientForOwner,
            workspaceId,
            startDate,
            endDate
        );
    }
    
    private const string SqlQuerySummaryByClientForOther = @"
        select
            c.id as ClientId,
            c.name as ClientName,
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
        inner join projects p on p.id = te.project_id
        inner join clients c on c.id = p.client_id 
        where te.project_id in (:projectIds) and te.date >= :startDate and te.date <= :endDate
        group by c.id, c.name
    ";
    
    public async Task<ICollection<ByClientsReportItemDto>> GetReportByClientForOtherAsync(
        DateTime startDate,
        DateTime endDate,
        long userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        return await GetReportForOtherAsync<ByClientsReportItemDto>(
            SqlQuerySummaryByClientForOther,
            startDate,
            endDate,
            userId,
            availableProjectsForUser
        );
    }
}
