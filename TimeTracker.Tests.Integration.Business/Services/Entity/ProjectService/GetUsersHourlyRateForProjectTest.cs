using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Entity.ProjectService;

public class GetUsersHourlyRateForProjectTest: BaseTest
{
    private readonly IRegistrationService _registrationService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IQueueService _queueService;
    private readonly IQueueDao _queueDao;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IProjectService _projectService;
    private readonly IProjectSeeder _projectSeeder;
    
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly ProjectEntity _project;

    public GetUsersHourlyRateForProjectTest(): base()
    {
        _registrationService = Scope.Resolve<IRegistrationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userDao = Scope.Resolve<IUserDao>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceSeeder = Scope.Resolve<IWorkspaceSeeder>();
        _queueService = Scope.Resolve<IQueueService>();
        _projectService = Scope.Resolve<IProjectService>();
        _queueDao = Scope.Resolve<IQueueDao>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _workspaceSeeder.CreateSeveralAsync(_user).Result.First();
        _project = _projectSeeder.CreateSeveralAsync(_workspace, _user).Result.First();
        
        _queueDao.CompleteAllPending();
    }

    [Fact]
    public async Task ShouldReturnHourlyRateForOwner()
    {
        var actualRate = await _projectService.GetUsersHourlyRateForProject(_user, _project);
        Assert.Equal(_project.DefaultHourlyRate, actualRate);
    }
    
    [Fact]
    public async Task ShouldReturnHourlyRateForUser()
    {
        var expectedRate = 15.78m;
        
        var userWithUserRole = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            userWithUserRole,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project,
                    HourlyRate = expectedRate
                }
            }
        );
        
        var actualRate = await _projectService.GetUsersHourlyRateForProject(userWithUserRole, _project);
        Assert.Equal(expectedRate, actualRate);
    }
    
    [Fact]
    public async Task ShouldReturnDefaultHourlyRateForUserIfNotProvidedInMembership()
    {
        var userWithUserRole = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            userWithUserRole,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project,
                    HourlyRate = null
                }
            }
        );
        
        var actualRate = await _projectService.GetUsersHourlyRateForProject(userWithUserRole, _project);
        Assert.Equal(_project.DefaultHourlyRate, actualRate);
    }
    
    [Fact]
    public async Task ShouldReturnHourlyRateForManager()
    {
        var expectedRate = 15.78m;
        
        var userWithUserRole = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            userWithUserRole,
            MembershipAccessType.Manager,
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project,
                    HourlyRate = expectedRate
                }
            }
        );
        
        var actualRate = await _projectService.GetUsersHourlyRateForProject(userWithUserRole, _project);
        Assert.Equal(expectedRate, actualRate);
    }
    
    [Fact]
    public async Task ShouldReturnDefaultHourlyRateForManagerIfNotProvidedInMembership()
    {
        var userWithUserRole = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            userWithUserRole,
            MembershipAccessType.Manager,
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project,
                    HourlyRate = null
                }
            }
        );
        
        var actualRate = await _projectService.GetUsersHourlyRateForProject(userWithUserRole, _project);
        Assert.Equal(_project.DefaultHourlyRate, actualRate);
    }
}
