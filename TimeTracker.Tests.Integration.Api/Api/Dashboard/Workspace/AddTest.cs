using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Workspace;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/add";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly string _jwtToken;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceFactory = ServiceProvider.GetRequiredService<IDataFactory<WorkspaceEntity>>();
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var project = _workspaceFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            Name = project.Name,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var workspace = _workspaceFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = workspace.Name
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceDto>();
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(workspace.Name, actual.Name);
        Assert.Equal(MembershipAccessType.Owner, actual.CurrentUserAccess);
        Assert.True(actual.IsCreatedByCurrentUser);
    }

    [Fact]
    public async Task ShouldNotAddMoreThanMaxActiveCustomWorkspaces()
    {
        await _workspaceSeeder.CreateSeveralAsync(_user, GlobalConstants.MaxActiveCreatedWorkspaces);
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            Name = "One more workspace"
        });

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new DataValidationException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldAddWhenOneOfCreatedWorkspacesIsDeleted()
    {
        var workspaces = await _workspaceSeeder.CreateSeveralAsync(_user, GlobalConstants.MaxActiveCreatedWorkspaces);
        await FlushDbChanges();

        var deletedWorkspace = workspaces.First();
        var deleteResponse = await PostRequestAsync("/dashboard/workspace/delete", _jwtToken, new DeleteRequest
        {
            WorkspaceId = deletedWorkspace.Id,
            ConfirmationName = deletedWorkspace.Name
        });
        deleteResponse.EnsureSuccessStatusCode();

        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            Name = "Replacement workspace"
        });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldNotCountWorkspacesCreatedByAnotherUser()
    {
        var otherUser = await UserSeeder.CreateActivatedAsync();
        var otherUsersWorkspaces = await _workspaceSeeder.CreateSeveralAsync(
            otherUser,
            GlobalConstants.MaxActiveCreatedWorkspaces
        );
        foreach (var workspace in otherUsersWorkspaces)
        {
            await _workspaceAccessService.ShareAccessAsync(workspace, _user, MembershipAccessType.Owner);
        }
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            Name = "My workspace"
        });

        response.EnsureSuccessStatusCode();
    }
}
