using System.Drawing;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tag;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/tag/update";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TagEntity> _tagFactory;
    private readonly string _jwtToken;
    private readonly ITagSeeder _tagSeeder;
    private readonly TagEntity _tag;
    private readonly IClientSeeder _clientSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly Color _expectedColor = Color.Blue;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _tagFactory = ServiceProvider.GetRequiredService<IDataFactory<TagEntity>>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _tag = _tagSeeder.CreateAsync(_workspace).Result;

    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectedProject = _tagFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            TagId = _tag.Id,
            Name = expectedProject.Name,
            Color = _expectedColor.ToHexString()
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var expectedTag = _tagFactory.Generate();
        await DbSessionProvider.PerformCommitAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TagId = _tag.Id,
            Name = expectedTag.Name,
            Color = _expectedColor.ToHexString()
        });
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<TagDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Equal(expectedTag.Name, actualProject.Name);
        Assert.Equal(_expectedColor.ToHexString(), actualProject.Color);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNoAccess()
    {
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAsync();
        await DbSessionProvider.PerformCommitAsync();
        
        var response = await PostRequestAsync(Url, otherJwtToken, new UpdateRequest()
        {
            TagId = _tag.Id,
            Name = _tag.Name,
        });
        
        var errorResponse = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), errorResponse.Type);
    }
}
