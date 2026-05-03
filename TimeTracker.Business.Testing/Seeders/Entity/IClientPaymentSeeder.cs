using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IClientPaymentSeeder: IDomainService
{
    Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1);

    Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, ClientEntity client, ProjectEntity? project, int count = 1);

    Task<ICollection<ClientPaymentEntity>> CreateSeveralAsync(int count = 1);
}
