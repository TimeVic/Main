using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.WorkspaceMember;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/member/delete";
    
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

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace.Mode = WorkspaceMode.Team;
        DbSessionProvider.CurrentSession.UpdateAsync(_workspace).Wait();

        _newUser = _userFactory.Generate();
        (_jwtTokenOtherUser, _otherUser, _) = UserSeeder.CreateAuthorizedAsync().Result;
        
        var projectsAccess = new List<ProjectAccessModel>()
        {
            new () { Project = _projectSeeder.CreateAsync(_workspace).Result },
            new () { Project = _projectSeeder.CreateAsync(_workspace).Result },
            new () { Project = _projectSeeder.CreateAsync(_workspace).Result },
        };
        FlushDbChanges().Wait();
        _membership = _workspaceAccessService.ShareAccessAsync(
            _workspace,
            _otherUser,
            MembershipAccessType.User,
            projectsAccess
        ).Result;
        FlushDbChanges().Wait();
    }

    [Fact]
    public async Task UserCanNotDeleteMemberInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            MemberId = _membership.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            MemberId = _membership.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdateToManagerRole()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            MemberId = _membership.Id,
        });
        response.EnsureSuccessStatusCode();

        var isExists = await DbSessionProvider.CurrentSession.Query<WorkspaceMemberEntity>()
            .Where(item => item.Id == _membership.Id)
            .AnyAsync();
        Assert.False(isExists);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfNotManager()
    {
        var response = await PostRequestAsync(Url, _jwtTokenOtherUser, new DeleteRequest()
        {
            MemberId = _membership.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
        
        var isExists = await DbSessionProvider.CurrentSession.Query<WorkspaceMemberEntity>()
            .Where(item => item.Id == _membership.Id)
            .AnyAsync();
        Assert.True(isExists);
    }
}
