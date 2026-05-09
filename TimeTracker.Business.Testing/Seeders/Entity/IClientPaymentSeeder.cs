using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IClientPaymentSeeder: IDomainService
{
    Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(int count = 1);

    Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(ClientEntity client, ProjectEntity? project, int count = 1);
}
