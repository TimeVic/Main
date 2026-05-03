using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface IClientPaymentDao: IDomainService
{
    Task<ClientPaymentEntity?> GetById(Guid? id);

    Task<ClientPaymentEntity> CreateAsync(
        WorkspaceEntity workspace,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId = null,
        string? description = null
    );

    Task<ClientPaymentEntity?> UpdateClientPaymentAsync(
        Guid paymentId,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId,
        string? description
    );

    Task<ListDto<ClientPaymentEntity>> GetListAsync(WorkspaceEntity workspace, int page);
}
