using System.Drawing;
using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface ITagDao: IDomainService
{
    Task<TagEntity> CreateAsync(
        WorkspaceEntity workspace,
        string name,
        Color? color = null
    );

    Task<TagEntity?> GetById(long? id);

    Task<ICollection<TagEntity>> GetList(WorkspaceEntity workspace);
}
