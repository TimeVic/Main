using System.Runtime.CompilerServices;
using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    
    private readonly WorkspaceEntity _workspace;
    private readonly UserEntity _user;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _workspaceDao.CreateWorkspaceAsync(_user, "Test").Result;
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter);

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
        
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.True(item.CreateTime > DateTime.MinValue);
            Assert.True(item.EndTime > TimeSpan.MinValue);
            Assert.NotNull(item.Project);
        });
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherNamespaces()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter);

        var user2 = await _userSeeder.CreateActivatedAsync();
        await _timeEntrySeeder.CreateSeveralAsync(user2.DefaultWorkspace, user2, 15);
        
        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldSortList()
    {
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1);

        var actualFirst = actualList.Items.First();
        var actualLast = actualList.Items.Last();
        Assert.True(actualFirst.StartTime > actualLast.StartTime);
    }
    
    [Fact]
    public async Task ShouldFilterByClient()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 9);

        var expectedProject = (await _projectSeeder.CreateSeveralAsync(_workspace, _user)).First();
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter, expectedProject);

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            ClientId = expectedProject.Client.Id
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterByProject()
    {
        var expectedCounter = 7;
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 9);

        var expectedProject = (await _projectSeeder.CreateSeveralAsync(_workspace, _user)).First();
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter, expectedProject);

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            ProjectId = expectedProject.Id
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBillable()
    {
        var expectedCounter = 7;
        foreach (var timeEntryEntity in await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 9))
        {
            timeEntryEntity.IsBillable = false;
        }
        
        foreach (var timeEntryEntity in await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, expectedCounter))
        {
            timeEntryEntity.IsBillable = true;
        }
        await CommitDbChanges();

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            IsBillable = true
        });
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldFilterBySearchString()
    {
        var expectedDescription = "Some fake desc";
        var user = await _userSeeder.CreateActivatedAsync();
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, user, 9)).First();
        expectedEntry.Description = expectedDescription;
        await CommitDbChanges();

        var actualList = await _timeEntryDao.GetListAsync(_workspace, 1, new FilterDataDto()
        {
            Search = "FAKE"
        });
        Assert.Equal(1, actualList.TotalCount);
        Assert.Equal(expectedEntry.Id, actualList.Items.First().Id);
    }
    
    [Fact]
    public async Task ShouldFilterByMember()
    {
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3);
        
        var expectedUser = await _userSeeder.CreateActivatedAndShareAsync(
            _workspace,
            access: MembershipAccessType.User
        );
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, expectedUser, 3);
        await CommitDbChanges();

        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            filter: new FilterDataDto()
            {
                 MemberId = expectedUser.Id
            }
        );
        
        Assert.Equal(3, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(item.User.Id == expectedUser.Id);
        });
    }
    
    [Fact]
    public async Task ShouldReturnTimeEntriesOnlyForSharedProjects()
    {
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, _user, 4);
        foreach (var project in projects)
        {
            await _timeEntrySeeder.CreateSeveralAsync(_workspace, _user, 3, project);
        }
        var expectedProject1 = projects.First();
        var expectedProject2 = projects.Last();
        var expectedUser = await _userSeeder.CreateActivatedAndShareAsync(
            _workspace,
            access: MembershipAccessType.User,
            projects: new List<ProjectEntity>()
            {
                expectedProject1,
                expectedProject2
            }
        );
        await _timeEntrySeeder.CreateSeveralAsync(_workspace, expectedUser, 3);
        await CommitDbChanges();

        var actualList = await _timeEntryDao.GetListAsync(
            _workspace, 
            1,
            user: expectedUser,
            accessType: MembershipAccessType.User
        );
        
        Assert.Equal(9, actualList.TotalCount);
        Assert.All(actualList.Items, item =>
        {
            Assert.True(
                item.Project.Id == expectedProject1.Id 
                || item.Project.Id == expectedProject2.Id
                || item.User.Id == expectedUser.Id
            );
        });
    }
}
