using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IPaymentSeeder: IDomainService
{
    Task<ICollection<PaymentEntity>> CreateSeveralAsync(UserEntity user, int count = 1);

    Task<ICollection<PaymentEntity>> CreateSeveralAsync(ClientEntity client, ProjectEntity? project, int count = 1);

    Task<ICollection<PaymentEntity>> CreateSeveralAsync(int count = 1);
}
