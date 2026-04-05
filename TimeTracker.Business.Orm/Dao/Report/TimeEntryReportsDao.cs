using Autofac;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto.Reports;

namespace TimeTracker.Business.Orm.Dao.Report;

public class TimeEntryReportsDao: BaseDao, ITimeEntryReportsDao
{
    public TimeEntryReportsDao(ILifetimeScope scope): base(scope)
    {
    }
    
    public async Task<ICollection<ProjectPaymentsReportItemDto>> GetProjectPaymentsReport(
        Guid workspaceId,
        Guid userId,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.ProjectPayments"))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("userId", userId)
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<ProjectPaymentsReportItemDto>())
            .ListAsync<ProjectPaymentsReportItemDto>();
    }
}
