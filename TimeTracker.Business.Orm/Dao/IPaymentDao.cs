using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IPaymentDao: IDomainService
{
    Task<PaymentEntity> Create(
        ClientEntity client,
        decimal amount,
        DateTime paymentTime,
        long? projectId,
        string? description
    );
}
