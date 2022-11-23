using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.WorkspaceMembership;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/membership/update";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    
    private readonly UserEntity _newUser;
    private readonly WorkspaceEntity _workspace;
    
    private string _jwtTokenOtherUser;
    private UserEntity _otherUser;
    private readonly WorkspaceMembershipEntity _membership;
    private readonly IProjectDao _projectDao;
    private readonly List<ProjectEntity> _projects;
    private WorkspaceEntity _otherWorkspace;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _newUser = _userFactory.Generate();

        (_jwtTokenOtherUser, _otherUser, _otherWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _membership = _workspaceAccessService.ShareAccessAsync(
            _workspace,
            _otherUser,
            MembershipAccessType.User
        ).Result;
        
        _projects = new List<ProjectEntity>()
        {
            _projectDao.CreateAsync(_workspace, "test 1").Result,
            _projectDao.CreateAsync(_workspace, "test 2").Result,
            _projectDao.CreateAsync(_workspace, "test 3").Result
        };
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = MembershipAccessType.Manager,
            ProjectsAccess = {}
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdateToManagerRole()
    {
        var expectAccess = MembershipAccessType.Manager;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MembershipProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.True(actualMembership.User.Id > 0);
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
        
        Assert.All(actualMembership.ProjectAccesses, item =>
        {
            Assert.Null(item.HourlyRate);
            Assert.NotNull(item.Project);
            Assert.True(item.Project.Id > 0);
        });
    }
    
    [Fact]
    public async Task ShouldUpdateSetProjectHourlyRatesForRoleRoleManager()
    {
        var expectAccess = MembershipAccessType.Manager;
        var expectHourlyRate = 99.9m;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MembershipProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true,
                    HourlyRate = expectHourlyRate
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.True(actualMembership.User.Id > 0);
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
        Assert.All(actualMembership.ProjectAccesses, item =>
        {
            Assert.NotNull(item.HourlyRate);
            Assert.Equal(expectHourlyRate, item.HourlyRate);
        });
    }
    
    [Fact]
    public async Task ShouldUpdateToUserRole()
    {
        var expectAccess = MembershipAccessType.User;
    
        await CommitDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MembershipProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.True(actualMembership.User.Id > 0);
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
        Assert.All(actualMembership.ProjectAccesses, item =>
        {
            Assert.NotEmpty(item.Project.Name);
            Assert.True(item.Project.Id > 0);
        });
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfNotManager()
    {
        var response = await PostRequestAsync(Url, _jwtTokenOtherUser, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = MembershipAccessType.User,
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }

    [Fact]
    public async Task ShouldNotAddProjectsFromAnotherWorkspace()
    {
        var anotherProject = await _projectDao.CreateAsync(_otherWorkspace, "other 1");
        var anotherProject2 = await _projectDao.CreateAsync(_otherWorkspace, "other 1");
        await CommitDbChanges();
        
        var expectAccess = MembershipAccessType.User;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects
                .Concat(new List<ProjectEntity>()
                {
                    anotherProject,
                    anotherProject2
                })
                .Select(item => new MembershipProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
    }
    
    [Fact]
    public async Task ShouldUpdateMembershipForOtherUser()
    {
        var expectAccess = MembershipAccessType.Manager;
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MembershipProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.Equal(_otherUser.Id, actualMembership.User.Id);
    }
    
    [Fact]
    public async Task ShouldNotDuplicateAccessItems()
    {
        var expectAccess = MembershipAccessType.Manager;
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects
                .Concat(_projects)
                .Select(item =>
                {
                    return new MembershipProjectAccessRequest()
                    {
                        ProjectId = item.Id,
                        HasAccess = true
                    };
                }).ToArray()
        });
        response.EnsureSuccessStatusCode();
        response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects
                .Concat(_projects)
                .Select(item =>
                {
                    return new MembershipProjectAccessRequest()
                    {
                        ProjectId = item.Id,
                        HasAccess = true
                    };
                }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
    }
}
