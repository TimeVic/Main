using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ISharedClientReportDao : IDomainService
{
    Task<SharedClientReportEntity?> GetByClientIdAsync(Guid clientId);

    Task<SharedClientReportEntity?> GetByTokenAsync(string token);

    Task<SharedClientReportEntity> CreateAsync(ClientEntity client, string token);

    Task SaveAsync(SharedClientReportEntity report);
}
