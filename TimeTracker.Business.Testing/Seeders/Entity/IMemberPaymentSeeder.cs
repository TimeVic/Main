using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IMemberPaymentSeeder: IDomainService
{
    Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(UserEntity user, int count = 1);
    
    Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, UserEntity user, int count = 1);

    Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(WorkspaceEntity workspace, UserEntity user, ClientEntity client, ProjectEntity? project, int count = 1);

    Task<ICollection<MemberPaymentEntity>> CreateSeveralAsync(int count = 1);
}
