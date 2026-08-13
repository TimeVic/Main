using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.WorkspaceMember;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/member/update";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    
    private readonly UserEntity _newUser;
    private readonly WorkspaceEntity _workspace;
    
    private string _jwtTokenOtherUser;
    private UserEntity _otherUser;
    private readonly WorkspaceMemberEntity _membership;
    private readonly IProjectSeeder _projectSeeder;
    private readonly List<ProjectEntity> _projects;
    private WorkspaceEntity _otherWorkspace;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        var workspaceDao = ServiceProvider.GetRequiredService<IWorkspaceDao>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        workspaceDao.SetModeAsync(_workspace, WorkspaceMode.Team).Wait();

        _newUser = _userFactory.Generate();

        (_jwtTokenOtherUser, _otherUser, _otherWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        workspaceDao.SetModeAsync(_otherWorkspace, WorkspaceMode.Team).Wait();

        _membership = _workspaceAccessService.ShareAccessAsync(
            _workspace,
            _otherUser,
            MembershipAccessType.User
        ).Result;
        
        _projects = new List<ProjectEntity>()
        {
            _projectSeeder.CreateAsync(_workspace).Result,
            _projectSeeder.CreateAsync(_workspace).Result,
            _projectSeeder.CreateAsync(_workspace).Result
        };
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task UserCanNotUpdateMemberInSoloWorkspace()
    {
        var workspaceDao = ServiceProvider.GetRequiredService<IWorkspaceDao>();
        await workspaceDao.SetModeAsync(_workspace, WorkspaceMode.Solo);
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = MembershipAccessType.Manager,
            ProjectsAccess = {}
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            MemberId = _membership.Id,
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
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MemberProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.NotEqual(Guid.Empty, actualMembership.Id);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.NotEqual(Guid.Empty, actualMembership.User.Id);
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
        
        Assert.All(actualMembership.ProjectAccesses, item =>
        {
            Assert.Null(item.HourlyRate);
            Assert.NotNull(item.Project);
            Assert.NotEqual(Guid.Empty, item.Project.Id);
        });
    }
    
    [Fact]
    public async Task ShouldUpdateSetProjectHourlyRatesForRoleRoleManager()
    {
        var expectAccess = MembershipAccessType.Manager;
        var expectHourlyRate = 99.9m;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MemberProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true,
                    HourlyRate = expectHourlyRate
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.NotEqual(Guid.Empty, actualMembership.Id);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.NotEqual(Guid.Empty, actualMembership.User.Id);
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
    
        await FlushDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MemberProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.NotEqual(Guid.Empty, actualMembership.Id);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.NotEqual(Guid.Empty, actualMembership.User.Id);
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
        Assert.All(actualMembership.ProjectAccesses, item =>
        {
            Assert.NotEmpty(item.Project.Name);
            Assert.NotEqual(Guid.Empty, item.Project.Id);
        });
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfNotManager()
    {
        var response = await PostRequestAsync(Url, _jwtTokenOtherUser, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = MembershipAccessType.Manager,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MemberProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
        
        await FlushAndRefreshEntity(_membership);
        Assert.Equal(MembershipAccessType.User, _membership.Access);
        Assert.Empty(_membership.ProjectAccesses);
    }

    [Fact]
    public async Task ShouldNotAddProjectsFromAnotherWorkspace()
    {
        var anotherProject = await _projectSeeder.CreateAsync(_otherWorkspace);
        var anotherProject2 = await _projectSeeder.CreateAsync(_otherWorkspace);
        await FlushDbChanges();
        
        var expectAccess = MembershipAccessType.User;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects
                .Concat(new List<ProjectEntity>()
                {
                    anotherProject,
                    anotherProject2
                })
                .Select(item => new MemberProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
    }
    
    [Fact]
    public async Task ShouldUpdateMembershipForOtherUser()
    {
        var expectAccess = MembershipAccessType.Manager;
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects.Select(item =>
            {
                return new MemberProjectAccessRequest()
                {
                    ProjectId = item.Id,
                    HasAccess = true
                };
            }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.NotEqual(Guid.Empty, actualMembership.Id);
        Assert.Equal(_otherUser.Id, actualMembership.User.Id);
    }
    
    [Fact]
    public async Task ShouldNotDuplicateAccessItems()
    {
        var expectAccess = MembershipAccessType.Manager;
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects
                .Concat(_projects)
                .Select(item =>
                {
                    return new MemberProjectAccessRequest()
                    {
                        ProjectId = item.Id,
                        HasAccess = true
                    };
                }).ToArray()
        });
        response.EnsureSuccessStatusCode();
        response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MemberId = _membership.Id,
            Access = expectAccess,
            ProjectsAccess = _projects
                .Concat(_projects)
                .Select(item =>
                {
                    return new MemberProjectAccessRequest()
                    {
                        ProjectId = item.Id,
                        HasAccess = true
                    };
                }).ToArray()
        });
        response.EnsureSuccessStatusCode();
    
        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.Equal(3, actualMembership.ProjectAccesses.Count);
    }
}
