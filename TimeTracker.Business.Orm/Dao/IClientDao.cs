using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IClientDao: IDomainService
{
    Task<ClientEntity> CreateAsync(WorkspaceEntity workspace, string name);

    Task<ListDto<ClientEntity>> GetListAsync(WorkspaceEntity workspace, int page);

    Task<ClientEntity?> GetById(long? clientId, WorkspaceEntity? workspace = null);
}
