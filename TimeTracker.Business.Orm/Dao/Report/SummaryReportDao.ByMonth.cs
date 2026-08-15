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
    public async Task<ICollection<ByMonthsReportItemDto>> GetReportByMonthAsync(
        Guid workspaceId,
        Guid userId,
        DateTime startDate,
        DateTime endDate
    )
    {
        return await GetReportAsync<ByMonthsReportItemDto>(
            "Report.SummaryByMonth",
            workspaceId,
            userId,
            startDate,
            endDate
        );
    }
}
