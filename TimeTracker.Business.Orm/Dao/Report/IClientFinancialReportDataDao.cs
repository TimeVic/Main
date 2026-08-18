using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Reports;
using TimeTracker.Business.Orm.Dto.Reports.WorkspaceFinancialSummary;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Report;

public interface IClientFinancialReportDataDao : IDomainService
{
    Task<ICollection<FinancialClientProjectItemDto>> GetProjectBreakdownAsync(ClientEntity client);

    Task<ICollection<ClientFinancialReportPaymentItemDto>> GetPaymentsAsync(ClientEntity client);

    Task<ListDto<ClientFinancialReportTaskItemDto>> GetTasksAsync(ProjectEntity project, int page);
}





