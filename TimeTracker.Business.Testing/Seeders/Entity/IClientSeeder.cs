using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IClientSeeder: IDomainService
{
    Task<ICollection<ClientEntity>> CreateSeveralAsync(UserEntity user, int count = 1);
    
    Task<ICollection<ClientEntity>> CreateSeveralAsync(int count = 1);
}
