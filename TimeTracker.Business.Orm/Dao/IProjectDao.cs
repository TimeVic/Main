using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface IProjectDao: IDomainService
{
    Task<ProjectEntity> CreateAsync(ClientEntity client, string name);
    
    Task<ProjectEntity?> GetById(Guid? projectId);

    Task<ListDto<ProjectEntity>> GetAvailableForUserListAsync(
        WorkspaceEntity workspace,
        UserEntity? user = null,
        MembershipAccessType? accessType = null
    );

    Task SoftDeleteAsync(ProjectEntity project);
}
