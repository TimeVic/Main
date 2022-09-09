using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IProjectDao: IDomainService
{
    Task<ProjectEntity> Create(WorkspaceEntity workspace, string name);

    Task<ICollection<ProjectEntity>> GetByUser(UserEntity user);

    Task<ListDto<ProjectEntity>> GetListAsync(WorkspaceEntity workspace, int page);
}
