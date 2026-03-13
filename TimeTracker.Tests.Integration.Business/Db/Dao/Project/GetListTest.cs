using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Project;

public class GetListTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectDao _projectDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserDao _userDao;

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _userDao = Scope.Resolve<IUserDao>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 7;
        await _projectSeeder.CreateSeveralAsync(_workspace, expectedCounter);

        await FlushDbChanges();
        var actualList = await _projectDao.GetAvailableForUserListAsync(_workspace);
        Assert.Equal(expectedCounter, actualList.TotalCount);
        
        Assert.All(actualList.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.NotEmpty(item.Name);
        });
    }
    
    [Fact]
    public async Task ShouldReceiveOnlyActiveProjects()
    {
        var expectedCounter = 7;
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, expectedCounter);
        var firstProject = projects.First();
        firstProject.IsArchived = true;
        
        await FlushDbChanges();
        var actualList = await _projectDao.GetAvailableForUserListAsync(_workspace);
        Assert.Equal(expectedCounter - 1, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherNamespaces()
    {
        var expectedCounter = 7;
        await _projectSeeder.CreateSeveralAsync(_workspace, expectedCounter);

        var user2 = await _userSeeder.CreateActivatedAsync();
        var user2Workspace = (await _userDao.GetUsersWorkspaces(user2, MembershipAccessType.Owner)).First();
        await _projectSeeder.CreateSeveralAsync(user2Workspace, 15);
        
        await FlushDbChanges();
        var actualList = await _projectDao.GetAvailableForUserListAsync(_workspace);
        Assert.Equal(expectedCounter, actualList.TotalCount);
    }
    
    [Fact]
    public async Task ShouldReceiveOnlySharedProjectsIfUserHasUserRole()
    {
        var expectedCounter = 7;
        var projects = await _projectSeeder.CreateSeveralAsync(_workspace, expectedCounter);

        var otherUser = await  _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new () { Project = projects.First() },
                new () { Project = projects.Last() }
            }
        );
        
        await FlushDbChanges();
        var actualList = await _projectDao.GetAvailableForUserListAsync(_workspace, otherUser);
        Assert.Equal(2, actualList.TotalCount);
        
        var otherUser2 = await  _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser2,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new () { Project = projects.First() },
                new () { Project = projects.Last() }
            }
        );
        
        await FlushDbChanges();
        actualList = await _projectDao.GetAvailableForUserListAsync(_workspace, otherUser2);
        Assert.Equal(2, actualList.TotalCount);
    }
}
