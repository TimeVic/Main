using System.Drawing;
using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface ITagDao: IDomainService
{
    Task<TagEntity> CreateAsync(
        WorkspaceEntity workspace,
        string name,
        Color? color = null
    );

    Task<TagEntity?> GetById(Guid? id);

    Task<ICollection<TagEntity>> GetList(WorkspaceEntity workspace);

    Task DeleteTag(TagEntity tag);
}
