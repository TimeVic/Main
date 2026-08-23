using Domain.Abstractions;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Dashboard;

public interface IDashboardDao : IDomainService
{
    Task<DashboardCountersDto> GetCountersAsync(WorkspaceEntity workspace);
}
