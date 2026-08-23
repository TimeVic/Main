using Autofac;
using NHibernate;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Dashboard;

public class DashboardDao(ILifetimeScope scope) : BaseDao(scope), IDashboardDao
{
    public async Task<DashboardCountersDto> GetCountersAsync(WorkspaceEntity workspace)
    {
        var rawCount = await Session.CreateSQLQuery(ReadSqlQuery("Dashboard.GetCounters"))
            .AddScalar("PendingApprovalsCount", NHibernateUtil.Int32)
            .SetParameter("workspaceId", workspace.Id)
            .SetParameter("statusPending", (int)TimeEntryStatus.Pending)
            .SetParameter("ownerAccessType", (int)MembershipAccessType.Owner)
            .UniqueResultAsync<int>();

        return new DashboardCountersDto
        {
            PendingApprovalsCount = rawCount
        };
    }
}
