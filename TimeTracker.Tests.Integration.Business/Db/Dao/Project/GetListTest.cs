using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Project;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectDao _projectDao;
    private readonly IProjectSeeder _projectSeeder;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _projectSeeder.CreateSeveralAsync(user, expectedCounter);

        var actualList = await _projectDao.GetListAsync(workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
        
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotEmpty(item.Name);
        });
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherNamespaces()
    {
        var expectedCounter = 7;
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = user.Workspaces.First();
        await _projectSeeder.CreateSeveralAsync(user, expectedCounter);

        var user2 = await _userSeeder.CreateActivatedAsync();
        await _projectSeeder.CreateSeveralAsync(user2, 15);
        
        var actualList = await _projectDao.GetListAsync(workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
}