using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IPaymentDao: IDomainService
{
    Task<PaymentEntity?> GetById(long? id);
    
    Task<PaymentEntity> CreateAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        long? projectId = null,
        string? description = null
    );

    Task<PaymentEntity?> UpdatePaymentAsync(
        long paymentId,
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        long? projectId,
        string? description
    );
    
    Task<ListDto<PaymentEntity>> GetListAsync(WorkspaceEntity workspace,  UserEntity user, int page);
}
