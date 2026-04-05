using Autofac;
using NHibernate;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto.Reports.Summary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public partial class SummaryReportDao: BaseDao, ISummaryReportDao
{
    public SummaryReportDao(ILifetimeScope scope): base(scope)
    {
    }

    private async Task<ICollection<T>> GetReportForOwnerOrManagerAsync<T>(
        string queryPath,
        Guid workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await Session.CreateSQLQuery(ReadSqlQuery(queryPath))
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.Date)
            .SetParameter("endDate", endDate.Date)
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
    
    public async Task<ICollection<T>> GetReportForOtherAsync<T>(
        string queryPath,
        DateTime startDate,
        DateTime endDate,
        Guid userId,
        IEnumerable<ProjectEntity>? availableProjectsForUser = null
    )
    {
        if (availableProjectsForUser == null)
        {
            throw new ArgumentNullException(nameof(availableProjectsForUser));
        }

        if (!availableProjectsForUser.Any())
        {
            return new List<T>();
        }
        return await Session.CreateSQLQuery(ReadSqlQuery(queryPath))
            .SetParameterList(
                "projectIds",
                availableProjectsForUser.Select(item => item.Id).ToArray()
            )
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetParameter("userId", userId)
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
}
