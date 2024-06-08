using TimeTracker.Business.Orm.Dao.FileStorage;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Entities.FileStorage;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;

namespace TimeTracker.Business.Testing.Seeders.Entity.FileStorage;

public class FileStorageBucketSeeder: IFileStorageBucketSeeder
{
    private readonly IDataFactory<FileStorageBucketEntity> _factory;
    private readonly IFileStorageBucketDao _storageBucketDao;
    private readonly IGoalsTrackerItemsDao factory;
    
    public FileStorageBucketSeeder(
        IDataFactory<FileStorageBucketEntity> factory,
        IFileStorageBucketDao storageBucketDao
    )
    {
        _factory = factory;
        _storageBucketDao = storageBucketDao;
    }

    public async Task<FileStorageBucketEntity> CreateAsync(UserEntity user)
    {
        var fakeEntry = _factory.Generate();
        return await _storageBucketDao.Create(
            user,
            fakeEntry.Name
        );
    }
}
