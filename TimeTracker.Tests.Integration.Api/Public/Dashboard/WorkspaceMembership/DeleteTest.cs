using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
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
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.WorkspaceMembership;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/membership/delete";
    
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

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _newUser = _userFactory.Generate();
        (_jwtTokenOtherUser, _otherUser, _) = UserSeeder.CreateAuthorizedAsync().Result;
        
        var projectsAccess = new List<ProjectAccessModel>()
        {
            new () { Project = _projectDao.CreateAsync(_workspace, "test 1").Result },
            new () { Project = _projectDao.CreateAsync(_workspace, "test 2").Result },
            new () { Project = _projectDao.CreateAsync(_workspace, "test 3").Result },
        };
        CommitDbChanges().Wait();
        _membership = _workspaceAccessService.ShareAccessAsync(
            _workspace,
            _otherUser,
            MembershipAccessType.User,
            projectsAccess
        ).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            MembershipId = _membership.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdateToManagerRole()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            MembershipId = _membership.Id,
        });
        response.EnsureSuccessStatusCode();

        var isExists = await DbSessionProvider.CurrentSession.Query<WorkspaceMembershipEntity>()
            .Where(item => item.Id == _membership.Id)
            .AnyAsync();
        Assert.False(isExists);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfNotManager()
    {
        var response = await PostRequestAsync(Url, _jwtTokenOtherUser, new DeleteRequest()
        {
            MembershipId = _membership.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
