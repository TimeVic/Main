using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tag;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/tag/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IDataFactory<TagEntity> _tagFactory;
    private readonly ITagSeeder _tagSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _tagFactory = ServiceProvider.GetRequiredService<IDataFactory<TagEntity>>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _tagSeeder.CreateSeveralAsync(_defaultWorkspace, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotEmpty(item.Name);
            Assert.NotEmpty(item.Color);
        });
    }
}
