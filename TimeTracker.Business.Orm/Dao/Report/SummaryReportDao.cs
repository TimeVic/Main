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
    private readonly IDbSessionProvider _sessionProvider;

    public SummaryReportDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    private async Task<ICollection<T>> GetReportForOwnerOrManagerAsync<T>(
        string query,
        long workspaceId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await _sessionProvider.CurrentSession.CreateSQLQuery(query)
            .SetParameter("workspaceId", workspaceId)
            .SetParameter("startDate", startDate.StartOfDay())
            .SetParameter("endDate", endDate.EndOfDay())
            .SetResultTransformer(Transformers.AliasToBean<T>())
            .ListAsync<T>();
    }
    
    public async Task<ICollection<T>> GetReportForOtherAsync<T>(
        string query,
        DateTime startDate,
        DateTime endDate,
        long userId,
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
        return await _sessionProvider.CurrentSession.CreateSQLQuery(query)
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
