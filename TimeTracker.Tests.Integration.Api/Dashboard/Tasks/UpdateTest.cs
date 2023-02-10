using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/project/update";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly string _jwtToken;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly IClientSeeder _clientSeeder;
    private readonly WorkspaceEntity _workspace;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectFactory = ServiceProvider.GetRequiredService<IDataFactory<ProjectEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _project = _projectSeeder.CreateAsync(_workspace).Result;
    }

    [Fact]
    public async System.Threading.Tasks.Task NonAuthorizedCanNotDoIt()
    {
        var expectedProject = _projectFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            ProjectId = _project.Id,
            Name = expectedProject.Name
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldUpdate()
    {
        var expectedClient = _clientSeeder.CreateSeveralAsync(_workspace).Result.First();
        var expectedProject = _projectFactory.Generate();
        await DbSessionProvider.PerformCommitAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            ProjectId = _project.Id,
            Name = expectedProject.Name,
            DefaultHourlyRate = expectedProject.DefaultHourlyRate,
            IsBillableByDefault = expectedProject.IsBillableByDefault,
            ClientId = expectedClient.Id
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<ProjectDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Equal(expectedProject.Name, actualProject.Name);
        Assert.Equal(expectedProject.DefaultHourlyRate, actualProject.DefaultHourlyRate);
        Assert.Equal(expectedProject.IsBillableByDefault, actualProject.IsBillableByDefault);
        Assert.Equal(expectedClient.Id, actualProject.Client.Id);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldSetClientToNull()
    {
        var expectedClient = _clientSeeder.CreateSeveralAsync(_workspace).Result.First();
        _project.SetClient(expectedClient);
        await DbSessionProvider.PerformCommitAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            ProjectId = _project.Id,
            Name = _project.Name,
            ClientId = null
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<ProjectDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Null(actualProject.Client);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldNotSetClientFromOtherUser()
    {
        var otherClient = _clientSeeder.CreateSeveralAsync().Result.First();
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            ProjectId = _project.Id,
            Name = _project.Name,
            ClientId = otherClient.Id
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<ProjectDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Null(actualProject.Client);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldNotUpdateIfHasNoAccess()
    {
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAsync();
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, otherJwtToken, new UpdateRequest()
        {
            ProjectId = _project.Id,
            Name = _project.Name
        });
        
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.Type);
    }
}
