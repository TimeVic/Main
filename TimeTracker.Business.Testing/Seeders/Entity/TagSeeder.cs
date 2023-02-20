using System.Drawing;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class TagSeeder: ITagSeeder
{
    private readonly IDataFactory<TagEntity> _tagFactory;
    private readonly ITagDao _tagDao;

    public TagSeeder(
        IDataFactory<TagEntity> tagFactory,
        ITagDao tagDao
    )
    {
        _tagFactory = tagFactory;
        _tagDao = tagDao;
    }

    public async Task<ICollection<TagEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1)
    {
        var result = new List<TagEntity>();
        for (int i = 0; i < count; i++)
        {
            result.Add(
                await CreateAsync(workspace)
            );
        }

        return result;
    }
    
    public async Task<TagEntity> CreateAsync(WorkspaceEntity workspace)
    {
        var fakeEntry = _tagFactory.Generate();
        var entry = await _tagDao.CreateAsync(
            workspace,
            fakeEntry.Name,
            Color.Brown
        );
        
        return entry;
    }
}
