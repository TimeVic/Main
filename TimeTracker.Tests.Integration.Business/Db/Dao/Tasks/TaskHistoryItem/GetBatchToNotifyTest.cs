using Autofac;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Tasks.TaskHistoryItem;

public class GetBatchToNotifyTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;
    private readonly ITaskSeeder _taskSeeder;
    
    private readonly UserEntity _user;

    public GetBatchToNotifyTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _taskSeeder = Scope.Resolve<ITaskSeeder>();
        _taskHistoryItemDao = Scope.Resolve<ITaskHistoryItemDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        
        // Refresh active
        _taskHistoryItemDao.GetBatchToNotify().Wait();
    }

    [Fact]
    public async Task ShouldGetToNotify()
    {
        var task = await _taskSeeder.CreateAsync();
        var actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify(0);
        Assert.Single(actualHistoryItems);
        
        actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify();
        Assert.Empty(actualHistoryItems);
    }
    
    [Fact]
    public async Task ShouldGetLastToNotify()
    {
        var task = await _taskSeeder.CreateAsync();
        await _taskHistoryItemDao.Create(task, _user);
        await _taskHistoryItemDao.Create(task, _user);
        
        var actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify(0);
        Assert.Single(actualHistoryItems);
        
        actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify();
        Assert.Empty(actualHistoryItems);
    }
    
    [Fact]
    public async Task ShouldGetLastForSeveralTasks()
    {
        var task1 = await _taskSeeder.CreateAsync();
        await _taskHistoryItemDao.Create(task1, _user);
        var task2 = await _taskSeeder.CreateAsync();
        await _taskHistoryItemDao.Create(task2, _user);
        
        var actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify(0);
        Assert.Equal(2, actualHistoryItems.Count);
        Assert.Contains(actualHistoryItems, item => item.Task.Id == task1.Id || item.Task.Id == task2.Id);
    }
    
    [Fact]
    public async Task ShouldGetLastWithTimeout()
    {
        var expetedTimeOut = 30;
        var task1 = await _taskSeeder.CreateAsync();
        var firstItem = await _taskHistoryItemDao.Create(task1, _user);
        firstItem.CreateTime = firstItem.CreateTime.AddSeconds(-expetedTimeOut);
        
        var secondItem = await _taskHistoryItemDao.Create(task1, _user);
        secondItem.CreateTime = DateTime.UtcNow;
        await CommitDbChanges();
        
        var actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify(30);
        Assert.Equal(1, actualHistoryItems.Count);
        Assert.All(actualHistoryItems, item =>
        {
            Assert.Equal(firstItem.Id, item.Id);
        });
        
        actualHistoryItems = await _taskHistoryItemDao.GetBatchToNotify(0);
        Assert.Equal(1, actualHistoryItems.Count);
        Assert.All(actualHistoryItems, item =>
        {
            Assert.Equal(secondItem.Id, item.Id);
        });
    }
}
