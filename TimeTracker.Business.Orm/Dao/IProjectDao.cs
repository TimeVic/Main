using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IProjectDao: IDomainService
{
    Task<ProjectEntity> CreateAsync(WorkspaceEntity workspace, string name);
    
    Task<ProjectEntity?> GetById(long? projectId);

    Task<ListDto<ProjectEntity>> GetAvailableForUserListAsync(
        WorkspaceEntity workspace,
        UserEntity? user = null,
        MembershipAccessType? accessType = null
    );
}
