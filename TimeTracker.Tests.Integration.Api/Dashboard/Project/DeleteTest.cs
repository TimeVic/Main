using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Project;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/project/delete";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly string _jwtToken;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly IClientSeeder _clientSeeder;
    private readonly WorkspaceEntity _workspace;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectFactory = ServiceProvider.GetRequiredService<IDataFactory<ProjectEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _project = _projectSeeder.CreateAsync(_workspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            ProjectId = _project.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDelete()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            ProjectId = _project.Id,
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_project);
        Assert.True(_project.IsArchived);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfArchived()
    {
        _project.IsArchived = true;
        await DbSessionProvider.PerformCommitAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            ProjectId = _project.Id,
            Name = _project.Name,
            ClientId = Guid.Empty
        });
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfHasNoAccess()
    {
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAsync();
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, otherJwtToken, new DeleteRequest()
        {
            ProjectId = _project.Id
        });
        
        var errorResponse = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.ErrorCode);
    }
}
