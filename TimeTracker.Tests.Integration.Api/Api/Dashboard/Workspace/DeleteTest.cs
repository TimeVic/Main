using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Workspace;

public class DeleteTest : BaseTest
{
    private const string Url = "/dashboard/workspace/delete";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task ShouldSoftDeleteIfWorkspaceOwner()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();

        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest
        {
            WorkspaceId = workspace.Id,
            ConfirmationName = workspace.Name
        });

        response.EnsureSuccessStatusCode();
        await FlushDbChanges(true);

        var deletedWorkspace = await DbSessionProvider.CurrentSession.GetAsync<WorkspaceEntity>(workspace.Id);
        Assert.NotNull(deletedWorkspace?.DeletedAt);

        var listResponse = await PostRequestAsync("/dashboard/workspace/list", _jwtToken, new GetListRequest());
        var list = await listResponse.GetJsonDataAsync<PaginatedListDto<WorkspaceDto>>();
        Assert.DoesNotContain(list.Items, item => item.Id == workspace.Id);
    }

    [Fact]
    public async Task ShouldNotDeleteIfWorkspaceManager()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();
        var (managerToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(workspace, MembershipAccessType.Manager);

        var response = await PostRequestAsync(Url, managerToken, new DeleteRequest
        {
            WorkspaceId = workspace.Id,
            ConfirmationName = workspace.Name
        });

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotDeleteDefaultWorkspace()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest
        {
            WorkspaceId = _defaultWorkspace.Id,
            ConfirmationName = _defaultWorkspace.Name
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotDeleteWhenConfirmationDoesNotMatch()
    {
        var workspace = (await _workspaceSeeder.CreateSeveralAsync(_user)).Single();

        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest
        {
            WorkspaceId = workspace.Id,
            ConfirmationName = "incorrect"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
