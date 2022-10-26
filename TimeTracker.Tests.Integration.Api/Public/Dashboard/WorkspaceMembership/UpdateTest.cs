using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Mapping;
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

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.WorkspaceMembership;

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

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;

        _newUser = _userFactory.Generate();
        _workspace = _user.Workspaces.First();

        (_jwtTokenOtherUser, _otherUser) = UserSeeder.CreateAuthorizedAsync().Result;
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
            ProjectIds = null
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
            ProjectIds = _projects.Select(item => item.Id).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.True(actualMembership.User.Id > 0);
        Assert.Equal(3, actualMembership.Projects.Count);
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
            ProjectIds = _projects.Select(item => item.Id).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(expectAccess, actualMembership.Access);
        Assert.True(actualMembership.User.Id > 0);
        Assert.Equal(3, actualMembership.Projects.Count);
        Assert.All(actualMembership.Projects, item =>
        {
            Assert.NotEmpty(item.Name);
            Assert.True(item.Id > 0);
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
        var anotherWorkspace = _otherUser.Workspaces.First();
        var anotherProject = await _projectDao.CreateAsync(anotherWorkspace, "other 1");
        var anotherProject2 = await _projectDao.CreateAsync(anotherWorkspace, "other 1");
        await CommitDbChanges();
        
        var expectAccess = MembershipAccessType.User;
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectIds = _projects.Select(item => item.Id)
                .Concat(new List<long>()
                {
                    anotherProject.Id,
                    anotherProject2.Id
                })
                .ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.Equal(3, actualMembership.Projects.Count);
    }
    
    [Fact]
    public async Task ShouldUpdateMembershipForOtherUser()
    {
        var expectAccess = MembershipAccessType.Manager;
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            MembershipId = _membership.Id,
            Access = expectAccess,
            ProjectIds = _projects.Select(item => item.Id).ToArray()
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.Equal(_otherUser.Id, actualMembership.User.Id);
    }
}
