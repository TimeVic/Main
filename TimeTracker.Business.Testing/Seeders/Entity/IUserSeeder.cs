using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IUserSeeder: IDomainService
{
    Task<UserEntity> CreatePendingAsync();
    
    Task<UserEntity> CreateActivatedAsync(string password = "test password");

    Task<ICollection<UserEntity>> CreateActivatedAsync(int counter, string password = "test password");
    
    Task<(string token, UserEntity user)> CreateAuthorizedAsync(string password = "test password");

    Task<UserEntity> CreateActivatedAndShareAsync(WorkspaceEntity workspace, MembershipAccessType access = MembershipAccessType.User);
}
