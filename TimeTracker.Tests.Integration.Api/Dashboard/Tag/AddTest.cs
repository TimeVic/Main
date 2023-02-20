using System.Drawing;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tag;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/tag/add";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TagEntity> _tagFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly Color _expectedColor;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _tagFactory = ServiceProvider.GetRequiredService<IDataFactory<TagEntity>>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _expectedColor = Color.Blue;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var project = _tagFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            Name = project.Name,
            WorkspaceId = _workspace.Id,
            Color = _expectedColor.ToHexString()
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var project = _tagFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = project.Name,
            WorkspaceId = _workspace.Id,
            Color = _expectedColor.ToHexString()
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<TagDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Equal(project.Name, actualProject.Name);
        Assert.Equal(_expectedColor.ToHexString(), actualProject.Color);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var tag = _tagFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = tag.Name,
            WorkspaceId = otherWorkspace.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
