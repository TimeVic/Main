using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IPaymentDao: IDomainService
{
    Task<PaymentEntity?> GetById(long? id);
    
    Task<PaymentEntity> CreateAsync(
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

    Task<bool> HasAccessAsync(UserEntity user, PaymentEntity? entity);

    Task<ListDto<PaymentEntity>> GetListAsync(WorkspaceEntity workspace, int page);
}
