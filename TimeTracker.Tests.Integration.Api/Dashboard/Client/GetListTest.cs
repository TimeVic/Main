using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Client;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/client/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IClientSeeder _clientSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _clientSeeder.CreateSeveralAsync(_user, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotEmpty(item.Name);
        });
    }
}
