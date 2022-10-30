using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
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

    public GetListTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        
        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 7;
        await _projectSeeder.CreateSeveralAsync(_workspace, expectedCounter);

        var actualList = await _projectDao.GetAvailableForUserListAsync(_workspace);
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
        await _projectSeeder.CreateSeveralAsync(_workspace, expectedCounter);

        var user2 = await _userSeeder.CreateActivatedAsync();
        await _projectSeeder.CreateSeveralAsync(user2.Workspaces.First(), 15);
        
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
        
        actualList = await _projectDao.GetAvailableForUserListAsync(_workspace, otherUser2);
        Assert.Equal(2, actualList.TotalCount);
    }
}
