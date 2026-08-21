using Autofac;
using NHibernate;
using NHibernate.Transform;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Reports;
using TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public class ClientFinancialReportDataDao(ILifetimeScope scope, IWorkspaceFinancialSummaryReportDao workspaceFinancialSummaryReportDao)
    : BaseDao(scope), IClientFinancialReportDataDao
{
    public async Task<ICollection<FinancialClientProjectItemDto>> GetProjectBreakdownAsync(ClientEntity client)
    {
        var projects = await workspaceFinancialSummaryReportDao.GetClientProjectBreakdownAsync(client.Workspace.Id);
        return projects.Where(item => item.ClientId == client.Id).ToList();
    }

    public async Task<ICollection<ClientFinancialReportPaymentItemDto>> GetPaymentsAsync(ClientEntity client)
    {
        return await Session.CreateSQLQuery(ReadSqlQuery("Report.ClientFinancialReport.ClientPayments"))
            .SetParameter("clientId", client.Id)
            .SetResultTransformer(Transformers.AliasToBean<ClientFinancialReportPaymentItemDto>())
            .ListAsync<ClientFinancialReportPaymentItemDto>();
    }

    public async Task<ListDto<ClientFinancialReportTaskItemDto>> GetTasksAsync(ProjectEntity project, int page)
    {
        var offset = PaginationUtils.CalculateOffset(page);
        var totalCount = Convert.ToInt32(await Session.CreateSQLQuery(ReadSqlQuery("Report.ClientFinancialReport.TasksCount"))
            .AddScalar("TotalCount", NHibernateUtil.Int32)
            .SetParameter("clientId", project.Client.Id)
            .SetParameter("projectId", project.Id)
            .UniqueResultAsync());

        var items = await Session.CreateSQLQuery(ReadSqlQuery("Report.ClientFinancialReport.Tasks"))
            .SetParameter("clientId", project.Client.Id)
            .SetParameter("projectId", project.Id)
            .SetFirstResult(offset)
            .SetMaxResults(PaginationUtils.DefaultPageSize)
            .SetResultTransformer(Transformers.AliasToBean<ClientFinancialReportTaskItemDto>())
            .ListAsync<ClientFinancialReportTaskItemDto>();

        return new ListDto<ClientFinancialReportTaskItemDto>(items, totalCount);
    }
}








