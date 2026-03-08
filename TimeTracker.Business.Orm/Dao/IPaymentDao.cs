using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface IPaymentDao: IDomainService
{
    Task<PaymentEntity?> GetById(Guid? id);
    
    Task<PaymentEntity> CreateAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId = null,
        string? description = null
    );

    Task<PaymentEntity?> UpdatePaymentAsync(
        Guid paymentId,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        Guid? projectId,
        string? description
    );
    
    Task<ListDto<PaymentEntity>> GetListAsync(WorkspaceEntity workspace,  UserEntity user, int page);
}
