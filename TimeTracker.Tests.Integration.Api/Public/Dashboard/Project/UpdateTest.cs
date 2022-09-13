using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Project;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/project/update";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly string _jwtToken;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly IClientSeeder _clientSeeder;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectFactory = ServiceProvider.GetRequiredService<IDataFactory<ProjectEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateSeveralAsync(_user).Result.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
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
    public async Task ShouldUpdate()
    {
        var expectedWorkspace = _user.DefaultWorkspace;
        var expectedClient = _clientSeeder.CreateSeveralAsync(expectedWorkspace).Result.First();
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
    public async Task ShouldSetClientToNull()
    {
        var expectedWorkspace = _user.DefaultWorkspace;
        var expectedClient = _clientSeeder.CreateSeveralAsync(expectedWorkspace).Result.First();
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
    public async Task ShouldNotSetClientFromOtherUser()
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
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var (otherJwtToken, _) = await UserSeeder.CreateAuthorizedAsync();
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
