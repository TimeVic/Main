using Autofac;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao;

record struct TestContext(
    bool IsBoolean,
    string SomeText
);

public class QueueDaoTest: BaseTest
{
    private readonly IQueueDao _queueDao;

    public QueueDaoTest(): base()
    {
        _queueDao = Scope.Resolve<IQueueDao>();
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task ShouldPushNewItem()
    {
        var testContext = new TestContext()
        {
            IsBoolean = true,
            SomeText = "Test text"
        };

        await _queueDao.Push(testContext, QueueChannel.Default);
    }
    
    [Fact]
    public async Task ShouldGetTopItem()
    {
        var testContext = new TestContext()
        {
            IsBoolean = true,
            SomeText = "Test text"
        };

        await _queueDao.Push(testContext, QueueChannel.Default);
        await _queueDao.Push(testContext, QueueChannel.Default);
        
        var actualItem1 = await _queueDao.GetTop(QueueChannel.Default);
        var actualItem2 = await _queueDao.GetTop(QueueChannel.Default);
        Assert.True(actualItem1.Id > 0);
        Assert.Equal(QueueStatus.InProcess, actualItem1.Status);
        Assert.True(actualItem2.Id > 0);
        
        Assert.True(actualItem2.CreateTime > actualItem1.CreateTime);
    }
    
    [Fact]
    public async Task ShouldGetSameItem()
    {
        var testContext = new TestContext()
        {
            IsBoolean = true,
            SomeText = "Test text"
        };

        await _queueDao.Push(testContext, QueueChannel.Default);
        await _queueDao.Push(testContext, QueueChannel.Default);
        
        var actualItem1 = await _queueDao.GetTop(QueueChannel.Default);
        var actualItem2 = await _queueDao.GetTop(QueueChannel.Default);
        Assert.NotEqual(actualItem1.Id, actualItem2.Id);
    }
    
    [Fact]
    public async Task ShouldMarkItemAsProcessed()
    {
        var testContext = new TestContext()
        {
            IsBoolean = true,
            SomeText = "Test text"
        };

        await _queueDao.Push(testContext, QueueChannel.Default);
        var actualItem = await _queueDao.GetTop(QueueChannel.Default);
        await _queueDao.MarkAsProcessed(actualItem);
        
        actualItem = await _queueDao.GetById(actualItem.Id);
        Assert.Equal(QueueStatus.Success, actualItem.Status);
    }
    
    [Fact]
    public async Task ShouldMarkItemAsProcessedWithError()
    {
        var expectedError = "Some error";
        var testContext = new TestContext()
        {
            IsBoolean = true,
            SomeText = "Test text"
        };

        await _queueDao.Push(testContext, QueueChannel.Default);
        var actualItem = await _queueDao.GetTop(QueueChannel.Default);
        await _queueDao.MarkAsProcessed(actualItem, expectedError);
        
        actualItem = await _queueDao.GetById(actualItem.Id);
        Assert.Equal(QueueStatus.Fail, actualItem.Status);
        Assert.Equal(expectedError, actualItem.Error);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfTryingToCompleteItemWhichWasCompleted()
    {
        var expectedError = "Some error";
        var testContext = new TestContext()
        {
            IsBoolean = true,
            SomeText = "Test text"
        };

        await _queueDao.Push(testContext, QueueChannel.Default);
        var actualItem = await _queueDao.GetTop(QueueChannel.Default);
        await _queueDao.MarkAsProcessed(actualItem);

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _queueDao.MarkAsProcessed(actualItem);
        });
    }
    
    [Fact]
    public async Task ShouldDoAnythingIfPendingNotFound()
    {
        var actualItem = await _queueDao.GetTop(QueueChannel.Default);
        Assert.Null(actualItem);
    }
}
