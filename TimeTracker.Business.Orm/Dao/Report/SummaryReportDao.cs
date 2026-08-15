using Autofac;
using NHibernate;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto.Reports.Summary;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: BaseDao, ISummaryReportDao
{
    public SummaryReportDao(ILifetimeScope scope): base(scope)
    {
    }

    private async Task<ICollection<T>> GetReportAsync<T>(
        string queryPath,
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery(queryPath))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("userId", userId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
}
